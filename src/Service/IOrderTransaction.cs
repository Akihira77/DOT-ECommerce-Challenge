using ECommerce.Types;

namespace ECommerce.Service;

public interface IOrderTransactionService
{
    //INFO: ADMIN


    //INFO: CUSTOMER
    Task<OrderTransaction> PayingOrder(
        CancellationToken ct,
        PayingOrderDTO data,
        Order o);
}
