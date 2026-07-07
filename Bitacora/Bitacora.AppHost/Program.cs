var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("RabbitMQConnection");

var apiService = builder.AddProject<Projects.Bitacora_ApiService>("apiservice")
    .WithReference(messaging);

builder.AddProject<Projects.Bitacora_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(messaging);

builder.Build().Run();
