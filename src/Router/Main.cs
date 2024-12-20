namespace ECommerce.Router;

public static class Router
{

    private readonly static string BASE_PATH = "/api";
    public static void MainRouter(this WebApplication app)
    {
        var api = app
            .MapGroup(BASE_PATH)
            .RequireRateLimiting("fixed");

        api
            .MapGroup("/customers")
            .RegisterCustomerRouter();
        api
            .MapGroup("/categories")
            .RegisterProductCategoryRouter();
        api
            .MapGroup("/products")
            .RegisterProductRouter();
        api
            .MapGroup("/carts")
            .RegisterCustomerCartRouter();
        api
            .MapGroup("/orders")
            .RegisterOrderRouter();
    }
}
