namespace DeliveryService.Repositories;

using System.Collections.Concurrent;

using OrderService.Messages;

public class OrderRepository
{
    private ConcurrentDictionary<Guid, OrderCreated> orders = new();

    public async Task AddOrderAsync(OrderCreated order)
    {
        await Task.Delay(5);

        this.orders.TryAdd(order.Id, order);
    }

    public async Task<OrderCreated?> GetOrderByIdAsync(Guid orderId)
    {
        await Task.Delay(5);

        return this.orders.TryGetValue(orderId, out var order)
            ? order
            : null;
    }
}