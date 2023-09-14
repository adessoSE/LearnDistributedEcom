namespace OrderService.Controllers;

using System;
using System.Threading.Tasks;

using Bogus;

using MassTransit;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OrderService.Entities;
using OrderService.Persistence;

using Message = OrderService.Messages;

[Route("Orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext context;
    private readonly IPublishEndpoint publishEndpoint;

    public OrdersController(OrderDbContext context, IPublishEndpoint publishEndpoint)
    {
        this.context = context;
        this.publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        return this.Ok(await context.Orders.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await context.Orders
            //.Include(o => o.Items)
            .FirstOrDefaultAsync(m => m.Id == id);

        return order is null ? NotFound() : this.Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody]Order order)
    {
        order.Id = Guid.NewGuid();
        context.Add(order);

        await context.SaveChangesAsync();

        await this.publishEndpoint.Publish(
            new Message.OrderCreated(
                order.Id,
                order.CustomerId,
                order.Items
                    .Select(i => new Message.OrderItem(i.ArticleId, i.ArcticleTitle, i.Price, i.Quantity))
                    .ToArray(),
                DateTime.Now));

        return this.Ok(order);
    }

    [HttpPost("Random")]
    public async Task<IActionResult> CreateRandom()
    {
        var orderItemsFaker = new Faker<OrderItem>()
            .RuleFor(o => o.ArticleId, f => f.Random.Guid())
            .RuleFor(o => o.ArcticleTitle, f => f.Commerce.ProductName())
            .RuleFor(o => o.Price, f => f.Random.Decimal(1, 100))
            .RuleFor(o => o.Quantity, f => f.Random.Int(1, 10)); 
        
        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => f.Random.Guid())
            .RuleFor(o => o.CustomerId, f => f.Random.Guid())
            .RuleFor(o => o.Items, f => orderItemsFaker.GenerateBetween(1, 10));

        var order = orderFaker.Generate();

        return await this.Create(order);
    }
}
