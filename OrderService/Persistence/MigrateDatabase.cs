namespace OrderService.Persistence;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Polly;

internal static class MigrateDatabase
{
    public static void MigrateDbContext<TContext>(this WebApplication app, Action<TContext, IServiceProvider>? seeder = null)
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();

        var seed = app.Configuration.GetValue("Database:Seed", false);
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<TContext>>();
        var context = services.GetRequiredService<TContext>();

        try
        {
            logger.LogInformation(
                "Migrating database associated with context {DbContextName}",
                typeof(TContext).Name);

            var retry = Policy.Handle<SqlException>()
                    .WaitAndRetry(new TimeSpan[]
                        {
                            TimeSpan.FromSeconds(3),
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(8)
                        });

            retry.Execute(() => MigrateAndSeed(seeder, seed, context, services, logger));

            logger.LogInformation(
                "Migrated database associated with context {DbContextName}",
                typeof(TContext).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occurred while migrating the database used on context {DbContextName}",
                typeof(TContext).Name);
        }
    }

    private static void MigrateAndSeed<TContext>(
        Action<TContext, IServiceProvider>? seeder,
        bool seed,
        TContext context,
        IServiceProvider services,
        ILogger logger)
        where TContext : DbContext
    {
        if (context.Database.IsRelational())
        {
            context.Database.Migrate();
        }

        if (!seed)
        {
            return;
        }

        logger.LogInformation("Seeding database");

        seeder?.Invoke(context, services);
    }
}
