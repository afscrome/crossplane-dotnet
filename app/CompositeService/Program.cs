using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

//TODO: Support command line args from https://github.com/crossplane/crossplane/blob/main/contributing/specifications/functions.md#configuration
// --debug
// --inseucre
// --tls-certs-dir

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        )
    .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation())
    .UseOtlpExporter()
    ;


builder.WebHost.ConfigureKestrel(serverOptions =>
{
    /*
    serverOptions.ListenUnixSocket("unix:///@crossplane/fn/default.sock", listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
    */
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<MyFunctionRunnerService>();
//app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
