using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public class RestockProductBackgroundService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IDbContextFactory<ApplicationDbContext> dbCtxFactory;
    private readonly ILogger<RestockProductBackgroundService> logger;
    private readonly TimeSpan interval = TimeSpan.FromHours(1);

    public RestockProductBackgroundService(
        ILogger<RestockProductBackgroundService> logger,
        IDbContextFactory<ApplicationDbContext> dbCtxFactory,
        IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.dbCtxFactory = dbCtxFactory;
        this.serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                this.logger.LogInformation($"Processing Product Restock");

                await this.ProcessExpiredOrders(stoppingToken);

                this.logger.LogInformation($"Complete Product Restock");

                await Task.Delay(this.interval, stoppingToken);
            }
            catch (System.Exception err)
            {
                this.logger.LogError($"Product Restock Error {err.Source}: {err.Message}");
            }
        }
    }

    private async Task ProcessExpiredOrders(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        // var scope = this.serviceProvider.CreateScope();
        // using var dbCtx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // using var tx = dbCtx.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        using var dbCtx = await this.dbCtxFactory.CreateDbContextAsync(ct);
        using var tx = await dbCtx.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, ct);
        try
        {
            var expiredOrders = await dbCtx
                .Orders
                .Select(o => new { o.Id, o.Deadline, o.OrderStatus, o.OrderItems })
                // .Include(o => o!.OrderItems)
                .Where(o => DateTime.Now >= o.Deadline
                            && o.OrderStatus.Equals(OrderStatus.WAITING_PAYMENT))
                .ToListAsync();

            if (expiredOrders.Any())
            {
                this.logger.LogInformation($"There are [{expiredOrders.Count}] WAITING_PAYMENT Orders are EXPIRED");

                var expiredOrderIds = expiredOrders
                    .Select(ot => ot.Id)
                    .ToList();
                await dbCtx
                    .Database
                    .ExecuteSqlRawAsync($@"
                        UPDATE Orders
                        SET OrderStatus = {(int)OrderStatus.EXPIRED}
                        WHERE Id IN ({string.Join(",", expiredOrderIds)})
                    ", ct);

                var productQuantityMap = expiredOrders.
                    SelectMany(o => o.OrderItems).
                    GroupBy(oi => oi.ProductId).
                    ToDictionary(g => g.Key, g => g.Sum(oi => oi.Quantity));
                var productIds = productQuantityMap.Keys.ToArray();
                var products = await dbCtx
                    .Products
                    .FromSqlRaw($@"
                            SELECT * FROM Products WITH (UPDLOCK, ROWLOCK)
                            WHERE Id IN ({string.Join(",", productIds)})
                        ")
                    .ToListAsync();
                foreach (var product in products)
                {
                    if (productQuantityMap.TryGetValue(product.Id, out var totalQuantity))
                    {
                        product.Stock += totalQuantity;
                    }
                }

                await tx.CommitAsync(ct);
                await dbCtx.SaveChangesAsync(ct);
            }
        }
        catch (System.Exception err)
        {
            await tx.RollbackAsync(ct);
            this.logger.LogError($"Error in {err.Source}: {err.Message}");
            throw;
        }
    }
}

