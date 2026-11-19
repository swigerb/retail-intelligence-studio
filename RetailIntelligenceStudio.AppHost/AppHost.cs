var builder = DistributedApplication.CreateBuilder(args);

// Add the .NET backend server
var server = builder.AddDotnetProject("server", "../RetailIntelligenceStudio.Server/RetailIntelligenceStudio.Server.csproj")
    .WithExternalHttpEndpoints();

// Add the React/Vite frontend - configure proxy to use Aspire service discovery
var webfrontend = builder.AddJavaScriptApp("webfrontend", "../frontend")
    .WithRunScript("dev")
    .WithBuildScript("build")
    .WithNpm(install: false)
    .WithReference(server)
    .WithHttpEndpoint(port: 5173, env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
