namespace DeliveryService.Consumers;

using System.Threading.Tasks;

using DeliveryService.Repositories;

using MassTransit;

using OrderService.Messages;

public class RegisterOrderConsumer : IConsumer<OrderCreated>
{
    private readonly OrderRepository orderRepository;
    private readonly ILogger<RegisterOrderConsumer> logger;

    public RegisterOrderConsumer(OrderRepository orderRepository, ILogger<RegisterOrderConsumer> logger)
    {
        this.orderRepository = orderRepository;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        await this.orderRepository.AddOrderAsync(context.Message);

        this.logger.LogInformation("Registered order {orderId}", context.Message.Id);
    }
}
