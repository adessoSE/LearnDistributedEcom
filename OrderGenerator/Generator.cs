namespace OrderGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bogus;

using Serilog;

internal static class Generator
{
    static Guid[] customerIds = new Guid[]
    {
                new Guid("{42C73928-2E2E-402E-9435-D986A48D64F4}"),
                new Guid("{5C26C3CF-F073-4FF2-A90D-78CAC6520AD6}"),
                new Guid("{3ED78970-1329-4B26-AA6E-57CB68B97C0B}")
    };

    static Faker<OrderItem> itemsGenerator = new Faker<OrderItem>()
        .StrictMode(true)
        .RuleFor(o => o.Price, f => f.Finance.Random.Decimal(0.49m, 99))
        .RuleFor(o => o.ArcticleTitle, f => f.Commerce.Product())
        .RuleFor(o => o.ArticleId, f => Guid.NewGuid())
        .RuleFor(o => o.Quantity, f => f.Random.Number(1, 5));

    static Faker<Order> orderGenerator = new Faker<Order>()
        .StrictMode(true)
        .RuleFor(o => o.CustomerId, f => f.PickRandom(customerIds))
        .RuleFor(o => o.Items, f => itemsGenerator.GenerateBetween(1, 5).ToArray());


    public static Order CreateOrder()
    {
        var order = orderGenerator.Generate();

        Log.Information($"Generated order with {order.Items.Length} items for customer {order.CustomerId:D}");

        return order;
    }
}
