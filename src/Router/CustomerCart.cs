using ECommerce.Handler;
using ECommerce.Middleware;

namespace ECommerce.Router;

public static class CustomerCartRouter
{
    public static void RegisterCustomerCartRouter(this RouteGroupBuilder r)
    {
        r.RequireAuthentication();
    }

    private static void RequireAuthentication(this RouteGroupBuilder r)
    {
        var requireAuthGroup = r.MapGroup("").
            AddEndpointFilterFactory(AuthenticationValidationMiddleware.RequireAuthentication);

        requireAuthGroup.MapGet("", CustomerCartHandler.FindMyCart);
        requireAuthGroup.MapPost("", CustomerCartHandler.AddItemToCart);
        requireAuthGroup.MapPatch("/{cartItemId}", CustomerCartHandler.EditItemQuantity);
        requireAuthGroup.MapDelete("/{cartItemId}", CustomerCartHandler.RemoveItemToCart);
    }
}
