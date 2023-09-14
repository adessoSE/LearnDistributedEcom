namespace OrderService.Entities;

public class OrderItem
{
    public Guid ArticleId { get; set; }
    public string ArcticleTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal PriceTotal => this.Price * this.Quantity;
    public int Quantity { get; set; }
}
