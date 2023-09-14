namespace DeliveryService.Consumers;

using System.Threading.Tasks;

using DeliveryService.Messages;
using DeliveryService.Repositories;

using MassTransit;

using PaymentService.Messages;

public class DeliverOrderConsumer : IConsumer<OrderPaymentSucceeded>
{
    private readonly ILogger<DeliverOrderConsumer> logger;
    private readonly OrderRepository orderRepository;

    public DeliverOrderConsumer(ILogger<DeliverOrderConsumer> logger, OrderRepository orderRepository)
    {
        this.logger = logger;
        this.orderRepository = orderRepository;
    }

    public async Task Consume(ConsumeContext<OrderPaymentSucceeded> context)
    {
        this.logger.LogInformation("Collecting items for Order {orderId}", context.Message.OrderId);

        await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(10, 30)));

        var order = await this.orderRepository.GetOrderByIdAsync(context.Message.OrderId);
        var itemsListText = order is null
            ? "!!!!UNBEKANNT!!!!"
            : string.Join("\n", order.Items.Select(i => $"{i.Quantity,2}x  {i.ArticleTitle,-30} {i.PriceTotal,8:n2}"));

        this.logger.LogInformation("Collected all items for order {orderId}\n{itemsListText}", context.Message.OrderId, itemsListText);


        await context.Publish(new OrderDelivered(context.Message.OrderId));
    }
}
