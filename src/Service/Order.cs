using System.Globalization;
using ECommerce.Service.Interface;
using ECommerce.Store;
using ECommerce.Types;
using ECommerce.Util;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext ctx;
    private readonly EmailBackgroundService emailBackgroundService;
    private readonly ILogger<OrderService> logger;

    public OrderService(
        ApplicationDbContext ctx,
        EmailBackgroundService emailBackgroundService,
        ILogger<OrderService> logger)
    {
        this.ctx = ctx;
        this.emailBackgroundService = emailBackgroundService;
        this.logger = logger;
    }

    public async Task<Order> CreateOrder(
        CancellationToken ct,
        CustomerOverviewDTO c,
        IEnumerable<CustomerCart> myCart)
    {
        using var tx = this.ctx.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            ct.ThrowIfCancellationRequested();

            var now = DateTime.Now;
            var o = new Order
            {
                CustomerId = c.id,
                OrderStatus = OrderStatus.WAITING_PAYMENT,
                CreatedAt = now,
                Deadline = now.AddDays(1),
                TotalAmount = myCart.Sum(cc => cc.Amount),
                Version = 1,
            };
            await this.ctx.Orders.AddAsync(o, ct);

            var productIds = myCart
                .Select(cc => cc.ProductId)
                .ToList();
            var products = await this.ctx.Products
                .FromSqlRaw($@"
                        SELECT * FROM Products WITH (UPDLOCK, ROWLOCK) 
                        WHERE Id IN ({string.Join(",", productIds)})"
                )
                .ToDictionaryAsync(p => p.Id, ct);

            foreach (var item in myCart)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    throw new Exception($"Product with ID {item.ProductId} not found.");
                }

                if (product.Stock < item.Quantity)
                {
                    throw new Exception($"Insufficient stock for Product ID {item.ProductId}");
                }

                product.Stock -= item.Quantity;
                o.OrderItems.Add(new OrderItem
                {
                    OrderId = o.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Amount = item.Amount
                });
            }

            await this.ctx.OrderItems.AddRangeAsync(o.OrderItems, ct);
            await this.ctx.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM CustomerCarts WHERE CustomerId = {c.id}", ct);
            await this.ctx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            this.emailBackgroundService.QueueEmail(new sendEmailData(c.email, $"Your Order with id: [{o.Id}]", $"Your order has been created. Please check http://localhost:8000/orders/{o.Id}"));
            return o;
        }
        catch (System.Exception err)
        {
            await tx.RollbackAsync(ct);
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }

    public async Task<Order?> FindMyOrderById(
        CancellationToken ct,
        int id,
        int customerId,
        bool track)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.Orders.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query
                .Include(o => o.OrderTransaction)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id.Equals(id) &&
                        o.CustomerId.Equals(customerId), ct);
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<Order>> FindMyOrderHistories(
        CancellationToken ct,
        int customerId,
        OrderStatus? os,
        bool track)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.Orders.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            if (os is not null)
            {
                query = query.Where(o => o.OrderStatus.Equals(os));
            }

            return await query
                .Include(o => o.OrderTransaction)
                .Where(o => o.CustomerId.Equals(customerId))
                .ToListAsync(ct);
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }

    public async Task<Order?> FindOrderById(
        CancellationToken ct,
        int id,
        bool track)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.Orders.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query
                .Include(o => o.Customer)
                .Include(o => o.OrderTransaction)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id.Equals(id), ct);
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<Order>> FindOrders(
        CancellationToken ct,
        OrderStatus? os,
        bool track)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.Orders.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            if (os is not null)
            {
                query = query.Where(o => o.OrderStatus.Equals(os));
            }

            return await query
                .Include(o => o.OrderTransaction)
                .Include(o => o.Customer)
                .ToListAsync(ct);
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }

    public async Task<Order> UpdateOrderStatus(
        CancellationToken ct,
        Order o,
        UpdateOrderDTO data)
    {
        try
        {
            var orderStatus = data.orderStatus.ToEnumOrThrow<OrderStatus>();
            var updatedOrder = this
                .ctx
                .Orders
                .FromSql($@"
                        UPDATE Orders 
                        SET OrderStatus = {orderStatus}, Version = Version + 1 
                        OUTPUT INSERTED.*
                        WHERE Id = {o.Id} AND Version = {o.Version}"
                )
                .AsEnumerable()
                .SingleOrDefault();

            if (updatedOrder is null)
            {
                throw new InvalidOperationException("The order was modified by another user.");
            }

            this.emailBackgroundService.QueueEmail(new sendEmailData(o.Customer!.Email, "Order Status Change", $"Your has change the status. Please check http://localhost:8000/orders"));
            return updatedOrder;
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }

    public async Task GenerateReport(
        CancellationToken ct,
        CustomerOverviewDTO c,
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            string subject = $"Transactions Report Periode [{startDate:yyyy-MM-dd}__{endDate:yyyy-MM-dd}]";
            string body = "Transactions Report";
            var orderTransactions = await this.ctx
                .OrderTransactions
                .Include(ot => ot.Order)
                .Where(ot => ot.Order!.CreatedAt >= startDate
                        && ot.Order!.CreatedAt < endDate)
                .AsNoTracking()
                .ToListAsync();

            var outputPath = Path
                .Combine("Reports", $"Report_[{startDate:yyyy-MM-dd}__{endDate:yyyy-MM-dd}].pdf");

            // Ensure the Reports directory exists
            Directory.CreateDirectory("Reports");

            using var writer = new PdfWriter(outputPath);
            using var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            document.Add(new Paragraph("Order Transactions Report").SetFontSize(18));
            document.Add(new Paragraph($"Period: [{startDate:yyyy/MM/dd} - {endDate:yyyy/MM/dd}]").SetFontSize(12));
            document.Add(new Paragraph($"Generated On: {DateTime.UtcNow}").SetFontSize(12));

            var table = new Table(6);
            table.AddHeaderCell("ID");
            table.AddHeaderCell("Customer ID");
            table.AddHeaderCell("Total Amount");
            table.AddHeaderCell("Date");
            table.AddHeaderCell("Payment Method");
            table.AddHeaderCell("Payment Status");

            foreach (var transaction in orderTransactions)
            {
                table.AddCell(transaction.Id.ToString());
                table.AddCell(transaction.Order!.CustomerId.ToString());
                table.AddCell(transaction.Order!.TotalAmount.ToString("C", CultureInfo.CreateSpecificCulture("id-ID"))); // Currency format
                table.AddCell(transaction.Order!.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                table.AddCell(transaction.PaymentMethod.ToString());
                table.AddCell(transaction.PaymentStatus.ToString());
            }

            document.Add(table);
            document.Close();

            this.emailBackgroundService.QueueEmail(new sendEmailData(c.email, subject, body, outputPath));

            return;
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }
}
