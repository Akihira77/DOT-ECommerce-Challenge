using ECommerce.Service.Interface;
using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public class OrderTransactionService : IOrderTransactionService
{
    private readonly ApplicationDbContext ctx;
    public OrderTransactionService(ApplicationDbContext ctx)
    {
        this.ctx = ctx;
    }

    //TODO: PERFORM CONDITION BASED PAYMENT METHOD
    public async Task<OrderTransaction> PayingOrder(
        CancellationToken ct,
        PaymentMethod pm,
        Order o)
    {
        using var tx = this.ctx.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            ct.ThrowIfCancellationRequested();

            o.OrderStatus = OrderStatus.PROCESS;
            this.ctx.Update(o);

            var ot = new OrderTransaction
            {
                OrderId = o.Id,
                Order = o,
                PaymentMethod = pm,
                PaymentStatus = PaymentStatus.SUCCESS
            };
            await this.ctx.OrderTransactions.AddAsync(ot, ct);


            await tx.CommitAsync(ct);
            await this.ctx.SaveChangesAsync(ct);

            return ot;
        }
        catch (System.Exception err)
        {
            await tx.RollbackAsync(ct);
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }
}
