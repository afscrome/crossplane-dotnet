using Apiextensions.Fn.Proto.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Text.Json;

// Docs: https://buf.build/crossplane/crossplane/docs/main:apiextensions.fn.proto.v1
public class MyFunctionRunnerService(ILogger<RunFunctionResponse> logger) : FunctionRunnerService.FunctionRunnerServiceBase
{
    public override async Task<RunFunctionResponse> RunFunction(RunFunctionRequest request, ServerCallContext context)
    {
        var response = new RunFunctionResponse
        {
            Context = request.Context,
            Desired = request.Desired ?? new(),
        };

        try
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                var requestBody = JsonFormatter.Default.Format(request);
                logger.LogDebug("RunFunction called with request: {Request}", requestBody);
            }

            var inputResource = request.Observed.Composite.Resource_;
            var apiVersion = inputResource.Fields["apiVersion"]?.StringValue;
            var kind = inputResource.Fields["kind"]?.StringValue;

            await ((apiVersion, kind) switch
            {
                ("example.myorg.io/v1", "XBuckets") => XBucketsV1(request, response, context.CancellationToken),
                _ => NotFound(apiVersion, kind, response)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in RunFunction");
            response.Results.Add(new Result
            {
                Severity = Severity.Fatal,
                Message = "An exception occurred while processing the request.",
            });

        }
        return response;
    }

    private Task XBucketsV1(RunFunctionRequest request, RunFunctionResponse response, CancellationToken cancellationToken)
    {
        var inputResource = request.Observed.Composite.Resource_;

        var region = inputResource.Fields["spec"].StructValue.Fields["region"].StringValue;
        var names = inputResource.Fields["spec"].StructValue.Fields["names"].ListValue.Values;

        foreach (var name in names)
        {
            var resourceStruct = JsonParser.Default.Parse<Struct>(JsonSerializer.Serialize(new
            {
                apiVersion = "s3.aws.upbound.io/v1beta1",
                kind = "Bucket",
                metadata = new
                {
                    annotations = new Dictionary<string, string>
                    {
                        ["crossplane.io/external-name"] = name.StringValue,
                    },
                }
            }));

            response.Desired.Resources.Add($"xbuckets-{name}", new Resource { Resource_ = resourceStruct });
        }

        return Task.CompletedTask;
    }

    private static Task NotFound(string? apiVersion, string? kind, RunFunctionResponse response)
    {
        response.Results.Add(new Result
        {
            Severity = Severity.Fatal,
            Message = $"No implementation for {apiVersion}/{kind}",
        });
        return Task.CompletedTask;
    }
}