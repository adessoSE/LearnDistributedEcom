namespace OrderService.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OrderService.Entities;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Order");

        builder
            .OwnsMany(order => order.Items, item =>
            {
                item.WithOwner().HasForeignKey("OrderId");
                item.Property<int>("Id");
                item.HasKey("Id");
            });

        //builder.HasMany(order => order.Items).WithOne().HasForeignKey("OrderId");
    }
}
