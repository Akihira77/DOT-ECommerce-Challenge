using ECommerce.Types;

namespace ECommerce.Service;

//TODO: THINK:
//1. DO WE HAVE CANCEL ORDER FEATURE
//
//
public interface IOrderService
{
    //NOTE: ADMIN
    Task<IEnumerable<Order>> FindOrders(
        CancellationToken ct,
        OrderStatus? os,
        bool track);
    Task<Order?> FindOrderById(
        CancellationToken ct,
        int id,
        bool track);
    Task<Order> UpdateOrderStatus(
        CancellationToken ct,
        Order o,
        UpdateOrderDTO data);

    //NOTE: CUSTOMER
    Task<Order> CreateOrder(
        CancellationToken ct,
        int customerId,
        IEnumerable<CustomerCart> myCart);
    Task<IEnumerable<Order>> FindMyOrderHistories(
        CancellationToken ct,
        int customerId,
        OrderStatus? os,
        bool track);
    Task<Order?> FindMyOrderById(
        CancellationToken ct,
        int id,
        int customerId,
        bool track);
}
