var builder = DistributedApplication.CreateBuilder(args);

// Add the .NET backend server
var server = builder.AddProject<Projects.RetailIntelligenceStudio_Server>("server")
    .WithExternalHttpEndpoints();

// Add the React/Vite frontend - configure proxy to use Aspire service discovery
var webfrontend = builder.AddNpmApp("webfrontend", "../frontend", "dev")
    .WithReference(server)
    .WithHttpEndpoint(port: 5173, env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
