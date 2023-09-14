namespace OrderGenerator;

public class Order
{
    public Guid CustomerId { get; set; }
    public OrderItem[] Items { get; set; } = Array.Empty<OrderItem>();
}
