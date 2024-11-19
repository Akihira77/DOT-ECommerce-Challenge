using ECommerce.Types;

namespace ECommerce.Service;

public interface IOrderTransactionService
{
    //INFO: ADMIN


    //INFO: CUSTOMER
    Task<OrderTransaction> PayingOrder(
        CancellationToken ct,
        PaymentMethod pm,
        Order o);
}
