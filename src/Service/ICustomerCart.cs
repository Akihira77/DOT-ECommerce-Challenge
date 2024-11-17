using ECommerce.Types;

namespace ECommerce.Service;

public interface ICustomerCartService
{
    Task<IEnumerable<CustomerCart>> FindItemsInMyCart(
        CancellationToken ct,
        int customerId);
    Task<CustomerCart?> FindCartItemById(
        CancellationToken ct,
        int cartItemId,
        bool track);
    Task<CustomerCart?> FindCartItemInMyCartById(
        CancellationToken ct,
        int customerId,
        int cartItemId,
        bool track,
        bool includeProduct);
    Task<IEnumerable<CustomerCart>> AddItemToCart(
        CancellationToken ct,
        int customerId,
        CustomerCartDTO item);
    Task<CustomerCart> EditItemQuantity(
        CancellationToken ct,
        int quantity,
        ChangeItemQuantity ciq,
        CustomerCart cc);
    Task<IEnumerable<CustomerCart>> RemoveItemFromCart(
        CancellationToken ct,
        CustomerCart item);
}
