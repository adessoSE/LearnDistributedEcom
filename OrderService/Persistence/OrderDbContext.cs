namespace OrderService.Persistence;

using System.Reflection;

using Microsoft.EntityFrameworkCore;

using OrderService.Entities;

public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders => this.Set<Order>();

	public OrderDbContext(DbContextOptions options)
		: base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
