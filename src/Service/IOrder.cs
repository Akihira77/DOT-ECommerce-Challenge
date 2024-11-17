using ECommerce.Types;

namespace ECommerce.Service;

//TODO: THINK:
//1. DO WE HAVE CANCEL ORDER FEATURE
//
//
public interface IOrderService
{
    Task<IEnumerable<Order>> FindOrders(
        CancellationToken ct,
        bool track);
    Task<Order?> FindOrderById(
        CancellationToken ct,
        int id,
        bool track);

    Task<Order> CreateOrder(
        CancellationToken ct,
        int customerId,
        CreateOrderDTO data);
    Task<IEnumerable<Order>> FindMyOrderHistories(
        CancellationToken ct,
        int customerId,
        bool track);
    Task<Order?> FindMyOrderById(
        CancellationToken ct,
        int id,
        int customerId,
        bool track);
}