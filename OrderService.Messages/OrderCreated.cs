namespace OrderService.Messages;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public record OrderCreated(
    Guid Id,
    Guid CustomerId,
    OrderItem[] Items,
    DateTime CreatedDate)
{
}
