using ECommerce.Handler;
using ECommerce.Middleware;

public static class ProductRouter
{
    public static void RegisterProductRouter(this RouteGroupBuilder r)
    {
        r.UnrequireAnythingRoutes();
        r.RequireAdminRoutes();
    }

    private static void UnrequireAnythingRoutes(this RouteGroupBuilder r)
    {
        r.MapGet("", ProductHandler.FindProducts);
        r.MapGet("/{id}", ProductHandler.FindProductById);
    }

    private static void RequireAdminRoutes(this RouteGroupBuilder r)
    {
        var requireAdminGroup = r.MapGroup("/admin").
            AddEndpointFilterFactory(AuthenticationValidationMiddleware.RequireAdmin);
        requireAdminGroup.MapPost("", ProductHandler.CreateProduct);
        requireAdminGroup.MapPatch("/{productId}", ProductHandler.EditProduct);
        requireAdminGroup.MapDelete("/{productId}", ProductHandler.DeleteProduct);
    }
}
