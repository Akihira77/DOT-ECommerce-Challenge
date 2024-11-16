using ECommerce.Handler;

namespace ECommerce;

public static class Router
{
    private readonly static string BASE_PATH = "/api";
    public static void MainRouter(this WebApplication app)
    {
        var api = app.MapGroup(BASE_PATH);
        var customerGroup = api.MapGroup("/customers");
        var productCategoryGroup = api.MapGroup("/categories");

        customerGroup.CustomerRouter();
        productCategoryGroup.ProductCategoryRouter();
    }

    private static void CustomerRouter(this RouteGroupBuilder r)
    {
        r.MapGet("", CustomerHandler.FindCustomers);
        r.MapGet("/id/{id}", CustomerHandler.FindCustomerById);
        r.MapGet("/name-or-email/{str}", CustomerHandler.FindCustomerByNameOrEmail);
        r.MapPost("/register", CustomerHandler.CreateCustomer);
        r.MapPost("/login", CustomerHandler.Login);
        r.MapPatch("", CustomerHandler.EditCustomer);

        r.MapPatch("/admin/upgrade-customer/{customerId}", CustomerHandler.UpgradeCustomerToAdmin);
        r.MapDelete("/admin/{customerId}", CustomerHandler.DeleteCustomer);
    }

    private static void ProductCategoryRouter(this RouteGroupBuilder r)
    {
        r.MapGet("", ProductCategoryHandler.FindProductCategories);
        r.MapGet("/{id}", ProductCategoryHandler.FindProductCategoryById);
        r.MapPost("", ProductCategoryHandler.CreateProductCategory);
        r.MapPatch("", ProductCategoryHandler.EditProductCategory);
        r.MapDelete("", ProductCategoryHandler.DeleteProductCategory);
    }
}
