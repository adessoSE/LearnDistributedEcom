namespace PaymentService.Consumers;

using System.Threading.Tasks;

using MassTransit;

using OrderService.Messages;

using PaymentService.Messages;

public class ProcessOrderPaymentConsumer : IConsumer<OrderCreated>
{
    private readonly ILogger<ProcessOrderPaymentConsumer> logger;

    public ProcessOrderPaymentConsumer(ILogger<ProcessOrderPaymentConsumer> logger)
    {
        this.logger = logger;
    }


    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var now = DateTime.Now;
        var elapsed = now - context.Message.CreatedDate;

        var orderId = context.Message.Id;
        var totalPrice = context.Message.Items.Select(x => x.PriceTotal).Sum();
        
        this.logger.LogInformation("Order {orderId:D} created at {createdDate} ({elapsed} ms ago) with {itemCount} items for a total of {totalPrice:n}",
                                   orderId,
                                   context.Message.CreatedDate,
                                   Math.Round(elapsed.TotalMilliseconds),
                                   context.Message.Items.Length,
                                   totalPrice);

        this.logger.LogInformation("Start Payment process of {totalPrice:n} for Order {orderId:D}", totalPrice, orderId);

        await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(1, 10)));

        var failed = Random.Shared.Next(1, 100) % 2 == 1;

        if (failed)
        {
            this.logger.LogWarning("Payment failed for order {orderId}", orderId);
            await context.Publish(new OrderPaymentFailed(orderId));
        }
        else
        {
            this.logger.LogInformation("Payment succeeded for order {orderId}", orderId);
            await context.Publish(new OrderPaymentSucceeded(orderId));
        }
    }
}
