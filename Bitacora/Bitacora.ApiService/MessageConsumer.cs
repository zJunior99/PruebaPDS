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