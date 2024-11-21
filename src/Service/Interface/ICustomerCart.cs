using ECommerce.Types;

namespace ECommerce.Service.Interface;

public interface ICustomerCartService
{
    IQueryable<CustomerCart> FindItemsInMyCart(
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
    Task<CustomerCart?> FindCartItemInMyCartByProductId(
        CancellationToken ct,
        int customerId,
        int productId,
        bool track,
        bool includeProduct);
    Task<bool> AddItemToCart(
        CancellationToken ct,
        int customerId,
        CustomerCartDTO item);
    Task<CustomerCart> EditItemQuantity(
        CancellationToken ct,
        int quantity,
        ChangeItemQuantity ciq,
        CustomerCart cc);
    Task<bool> RemoveItemFromCart(
        CancellationToken ct,
        CustomerCart item);
}
