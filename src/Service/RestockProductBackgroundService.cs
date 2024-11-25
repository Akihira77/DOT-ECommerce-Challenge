using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public class RestockProductBackgroundService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<RestockProductBackgroundService> logger;
    private readonly TimeSpan interval = TimeSpan.FromHours(1);

    public RestockProductBackgroundService(
        ILogger<RestockProductBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        this.logger = logger;
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

        using var scope = this.serviceProvider.CreateScope();
        var dbCtx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        using var tx = dbCtx.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            var expiredOrders = await dbCtx
                .Orders
                .Include(o => o!.OrderItems)
                .Where(o => DateTime.Now.CompareTo(o.Deadline) > 0
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

                var productIds = expiredOrders
                    .SelectMany(o => o.OrderItems)
                        .Select(oi => oi.ProductId)
                    .Distinct()
                    .ToArray();
                var products = await dbCtx
                    .Products
                    .FromSqlInterpolated($@"
                        SELECT * FROM Products WITH (UPDLOCK, ROWLOCK)
                        WHERE Id IN ({string.Join(",", productIds)})
                    ")
                    .ToListAsync();
                var productLookup = products
                    .ToDictionary(p => p.Id);
                var orderItems = expiredOrders
                    .SelectMany(o => o.OrderItems)
                    .ToList();
                foreach (var orderItem in orderItems)
                {
                    if (productLookup.TryGetValue(orderItem.ProductId, out var product))
                    {
                        product.Stock += orderItem.Quantity;
                    }
                }

                await dbCtx.SaveChangesAsync();
                await tx.CommitAsync(ct);
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

