namespace OrderService.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public record OrderItem(
    Guid ArticleId,
    string ArticleTitle,
    decimal Price,
    int Quantity)
{
    public decimal PriceTotal =>  this.Price * this.Quantity;
}
