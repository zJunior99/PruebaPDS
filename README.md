[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/rHd4fXBF)
[![Open in Codespaces](https://classroom.github.com/assets/launch-codespace-2972f46106e565e64193e422d61a12cf1da4916b45550586e14ef0a7c637dd04.svg)](https://classroom.github.com/open-in-codespaces?assignment_repo_id=24198889)
# SESION DE LABORATORIO N° 08: Construyendo una aplicación con Comunicación Asincrona utilizando un Servicio de Mensajeria

## OBJETIVOS
  * Comprender el desarrollo de una aplicación con comunicación asincrona utilizando un servicio de mensajeria

## REQUERIMIENTOS
  * Conocimientos: 
    - Conocimientos básicos de SQL.
    - Conocimientos shell y comandos en modo terminal.
  * Hardware:
    - Virtualization activada en el BIOS.
    - CPU SLAT-capable feature.
    - Al menos 4GB de RAM.
  * Software:
    - Windows 10 64bit: Pro, Enterprise o Education (1607 Anniversary Update, Build 14393 o Superior)
    - Docker Desktop 
    - Powershell versión 7.x
    - .Net 6 o superior

## CONSIDERACIONES INICIALES
  * Clonar el repositorio mediante git para tener los recursos necesaarios
  * Verificar que otro servicio no este utilizando el puerto 1433. Desactivarlo si existiese.

## DESARROLLO
1. Iniciar la aplicación Docker Desktop:
2. Iniciar la aplicación Powershell o Windows Terminal en modo administrador.
3. En el terminal, ubicarse en un ruta que no sea del sistema. Ejecutar el siguiente comando para agregar la carga de trabajo de Microsoft Aspire.
```Powershell
dotnet workload install aspire
```
4. En el terminal, ejecutar el siguiente comando para crear la nueva aplicación.
```Powershell
dotnet new aspire-starter -o Bitacora
cd Bitacora
dotnet add .\Bitacora.Web\ package MassTransit.RabbitMQ
dotnet add .\Bitacora.ApiService\ package MassTransit.RabbitMQ
dotnet add .\Bitacora.AppHost\  package Aspire.Hosting.RabbitMQ
code .
```
5. En el Visual Studio Code, en el proyecto Bitacora.ServiceDefaults, adicionar el archivo MessageContract.cs, y adicionar el siguiente contenido:
```CSharp
namespace Bitacora.ServiceDefaults;
public record MessageContract
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime CreationDate { get; init; } = DateTime.UtcNow;

    public string Message { get; init; } = "";
}
```
6. En el Visual Studio Code, en el proyecto Bitacora.ApiService, adicionar el archivo MessageConsumer.cs, y adicionar el siguiente contenido:
```CSharp
using MassTransit;
using Bitacora.ServiceDefaults;
namespace Bitacora.ApiService;
public class MessageConsumer : IConsumer<MessageContract>
{
    public async Task Consume(ConsumeContext<MessageContract> context)
    {
        Console.WriteLine($"Received: {context.Message.Message}");
        await Task.CompletedTask;
    }
}
```
7. En el Visual Studio Code, en el proyecto Bitacora.AppHost, modificar el archivo Program.cs, y adicionar el siguiente contenido:
```CSharp
var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("RabbitMQConnection"); //Define el servicio de colas

var apiService = builder.AddProject<Projects.Bitacora_ApiService>("apiservice")
    .WithReference(messaging); // Inyecta el servicio en la capa api

builder.AddProject<Projects.Bitacora_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(messaging); // Inyecta el servicio en la capa web

builder.Build().Run();
```
8. En el Visual Studio Code, en el proyecto Bitacora.ApiService, modificar el archivo Program.cs, y adicionar el siguiente contenido:
```CSharp
using Bitacora.ApiService; //al inicio
using MassTransit; //al inicio

// antes de la linea `var app = builder.Build();`
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<MessageConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        var configuration = context.GetRequiredService<IConfiguration>();
        var host = configuration.GetConnectionString("RabbitMQConnection");
        cfg.Host(host);
        cfg.ConfigureEndpoints(context);
    });
});
```
9. En el Visual Studio Code, en el proyecto Bitacora.Web, modificar el archivo Program.cs, y adicionar el siguiente contenido:
```CSharp
using MassTransit; //al inicio

// antes de la linea `var app = builder.Build();`
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        var configuration = context.GetRequiredService<IConfiguration>();
        var host = configuration.GetConnectionString("RabbitMQConnection");
        cfg.Host(host);
        cfg.ConfigureEndpoints(context);
    });
});
```
10. En el Visual Studio Code, en el proyecto Bitacora.Web, modificar el archivo _Imports.razor, y adicionar el siguiente contenido al final:
```Razor
@using MassTransit
@using Bitacora.ServiceDefaults
```

11. En el Visual Studio Code, en el proyecto Bitacora.Web, modificar el archivo Home.razor, y adicionar el siguiente contenido al final:
```Razor
@page "/"
@rendermode InteractiveServer
@inject IBus Bus

<PageTitle>Home</PageTitle>

<InputText @bind-Value="Mensaje" />
<button class="btn btn-primary mt-4" @onclick="DispatchMessage">Enviar</button>

<h1>Hello, world!</h1>

Welcome to your new app.


@code {
    private string Mensaje { get; set; } = "";
    private async Task DispatchMessage()
    {
        await Bus.Publish(new MessageContract
        {
            Message = Mensaje
        });
    }
}
```

12. En el terminal, ejecutar el siguiente comando para inicializar la migración del mapeo objeto relacional.
```Powershell
cd UsuarioService/src/UsuarioService
dotnet ef migrations add v1
```
13. En el terminal, ejecutar el comando `dotnet run --project Bitacora.AppHost`. 

![image](https://github.com/UPT-FAING-EPIS/lab_arch_05/assets/10199939/3a31a676-6c3e-4219-a941-40928539d22f)

14. En el terminal, ubicar la linea `Login to the dashboard at https://localhost:XXXXX/login?t=token_autogenerado`, copiarla y Abrir un navegador de internet y pegarla

![image](https://github.com/UPT-FAING-EPIS/lab_arch_05/assets/10199939/b34e579d-e6d9-4690-81d0-b32d4d8e4707)

15. En el navegador de internet, dirigerse a la opción Estructurado y esperar hasta que los mensajes Bus started aparezcan

![image](https://github.com/UPT-FAING-EPIS/lab_arch_05/assets/10199939/64fa0889-44f8-4d93-be7c-d9044b09a4d8)

16. En el navegador de internet, dirigerse a la opción Recursos y hacer click en la url de Bitacora.Web, lo cual abrira una nueva pestaña. Volver a la pestaña anterior de Recursos y hacer click en Ver Registros de Bitacora ApiService.

![image](https://github.com/UPT-FAING-EPIS/lab_arch_05/assets/10199939/58de54c1-2f2c-43aa-8ddf-27e63b302951)

17. En el navegador de internet, volver a la pesataña de aplicación web y en el cuadro de texto ingresar un mensaje y presionar el boton Enviar. Luego volver a la pestaña anterior y podra visualizar el mensaje enviado.

![image](https://github.com/UPT-FAING-EPIS/lab_arch_05/assets/10199939/5f56c9f5-3753-49e5-b8fb-f2979980d536)

![image](https://github.com/UPT-FAING-EPIS/lab_arch_05/assets/10199939/2c375a4a-5cf7-4759-87ce-a076de9db902)

