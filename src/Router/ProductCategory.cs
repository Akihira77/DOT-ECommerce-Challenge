using ECommerce.Handler;
using ECommerce.Middleware;

public static class ProductCategoryRouter
{
    public static void RegisterProductCategoryRouter(this RouteGroupBuilder r)
    {
        r.UnrequireAnythingRoutes();
        r.RequireAdminRoutes();
    }

    private static void UnrequireAnythingRoutes(this RouteGroupBuilder r)
    {
        r.MapGet("", ProductCategoryHandler.FindProductCategories);
        r.MapGet("/{id}", ProductCategoryHandler.FindProductCategoryById);
    }

    private static void RequireAdminRoutes(this RouteGroupBuilder r)
    {
        var requireAdminGroup = r.MapGroup("/admin").
            AddEndpointFilterFactory(AuthenticationValidationMiddleware.RequireAdmin);
        requireAdminGroup.MapPost("", ProductCategoryHandler.CreateProductCategory);
        requireAdminGroup.MapPatch("/{categoryId}", ProductCategoryHandler.EditProductCategory);
        requireAdminGroup.MapDelete("/{categoryId}", ProductCategoryHandler.DeleteProductCategory);
    }
}
