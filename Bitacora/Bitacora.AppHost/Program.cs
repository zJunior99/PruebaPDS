var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("RabbitMQConnection"); //Define el servicio de colas

var apiService = builder.AddProject<Projects.Bitacora_ApiService>("apiservice")
    .WithReference(messaging); // Inyecta el servicio en la capa api

builder.AddProject<Projects.Bitacora_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(messaging); // Inyecta el servicio en la capa web

builder.Build().Run();
