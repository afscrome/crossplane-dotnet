var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.CompositeService>("apiservice");

builder.Build().Run();
