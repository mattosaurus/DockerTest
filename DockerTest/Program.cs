using DockerTest.Features.HealthCheck;
using IpRegistry.Extensions;
using MetOfficeDataPoint.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using Serilog;
using Serilog.Events;
using System.Net;
using System.Text;
using System.Text.Json;

namespace DockerTest
{
    class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");

                Log.Information("Starting web host");
                CreateWebApplication(args);

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void CreateWebApplication(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, configuration) => configuration
                .WriteTo.Console()
                .ReadFrom.Configuration(context.Configuration));

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Any, 8880, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    listenOptions.UseHttps();
                });
                options.Listen(IPAddress.Any, 7183, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    listenOptions.UseHttps();
                });
                options.Listen(IPAddress.Any, 5183, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
            });

            builder.Services.AddHealthChecks()
                .AddSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"));

            BuildServices(builder.Services);
            var app = builder.Build();

            BuildApp(app);

            app.Run();
        }

        private static void BuildServices(IServiceCollection services)
        {
            services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddMetOfficeDataPointService(Configuration["MetOfficeDataPoint:ApiKey"]);
            services.AddIpRegistry(options =>
            {
                options.ApiKey = Configuration["IpRegistry:ApiKey"];
            });

            services.AddHealthChecks()
                .AddCheck<MetOfficeDataPointHealthCheck>("MetOfficeDataPoint")
                .AddCheck<IpRegistryHealthCheck>("IpStack")
                .ForwardToPrometheus();
        }

        private static void BuildApp(WebApplication app)
        {
            app.MapHealthChecks("/healthcheck", new HealthCheckOptions
            {
                ResponseWriter = WriteResponse
            }).RequireHost("*:8880");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.UseRouting();

            app.UseWhen(context => context.Request.Path.StartsWithSegments("/weatherforecast"), appBuilder =>
            {
                appBuilder.UseHttpMetrics();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMetrics();
            });
        }

        static Task WriteResponse(HttpContext context, HealthReport healthReport)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var options = new JsonWriterOptions { Indented = true };

            using var memoryStream = new MemoryStream();
            using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WriteString("status", healthReport.Status.ToString());
                jsonWriter.WriteStartObject("results");

                foreach (var healthReportEntry in healthReport.Entries)
                {
                    jsonWriter.WriteStartObject(healthReportEntry.Key);
                    jsonWriter.WriteString("status",
                        healthReportEntry.Value.Status.ToString());
                    jsonWriter.WriteString("description",
                        healthReportEntry.Value.Description);
                    jsonWriter.WriteStartObject("data");

                    foreach (var item in healthReportEntry.Value.Data)
                    {
                        jsonWriter.WritePropertyName(item.Key);

                        JsonSerializer.Serialize(jsonWriter, item.Value,
                            item.Value?.GetType() ?? typeof(object));
                    }

                    jsonWriter.WriteEndObject();
                    jsonWriter.WriteEndObject();
                }

                jsonWriter.WriteEndObject();
                jsonWriter.WriteEndObject();
            }

            return context.Response.WriteAsync(
                Encoding.UTF8.GetString(memoryStream.ToArray()));
        }
    }
}