using ECommerce.Handler;
using ECommerce.Middleware;

namespace ECommerce.Router;

public static class OrderRouter
{
    public static void RegisterOrderRouter(this RouteGroupBuilder r)
    {
        r.RequireAuthentication();
        r.RequireAdminRoutes();
    }

    private static void RequireAuthentication(this RouteGroupBuilder r)
    {
        var requireAuthenticationGroup = r.MapGroup("").
            AddEndpointFilterFactory(AuthenticationValidationMiddleware.RequireAuthentication);

        requireAuthenticationGroup.MapGet("", OrderHandler.FindMyOrderHistories);
        requireAuthenticationGroup.MapGet("/{id}", OrderHandler.FindMyOrderById);
        requireAuthenticationGroup.MapPost("", OrderHandler.CreatePaymentIntent);
        requireAuthenticationGroup.MapPost("/paying/{orderId}", OrderHandler.OrderPaymentHook);
    }

    private static void RequireAdminRoutes(this RouteGroupBuilder r)
    {
        var requireAdminGroup = r.MapGroup("/admin").
            AddEndpointFilterFactory(AuthenticationValidationMiddleware.RequireAdmin);

        requireAdminGroup.MapGet("", OrderHandler.FindOrders);
        requireAdminGroup.MapGet("/report/generate", OrderHandler.GenerateReport);
        requireAdminGroup.MapGet("/{id}", OrderHandler.FindOrderById);
        requireAdminGroup.MapPatch("/{orderId}", OrderHandler.UpdateOrderStatus);
    }
}

