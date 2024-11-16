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
        //INFO: NO NEED AUTHENTICATION
        r.MapGet("", CustomerHandler.FindCustomers);
        r.MapGet("/id/{id}", CustomerHandler.FindCustomerById);
        r.MapGet("/name-or-email/{str}", CustomerHandler.FindCustomerByNameOrEmail);
        r.MapPost("/register", CustomerHandler.CreateCustomer);
        r.MapPost("/login", CustomerHandler.Login);

        //INFO: NEED AUTHENTICATION
        r.MapGet("/my-info", CustomerHandler.FindMyCustomerInfo);
        r.MapPatch("", CustomerHandler.EditCustomer);
        r.MapPost("/addresses", CustomerHandler.AddCustomerAddress);

        //INFO: ADMIN ONLY
        r.MapPatch("/admin/upgrade-customer/{customerId}", CustomerHandler.UpgradeCustomerToAdmin);
        r.MapDelete("/admin/{customerId}", CustomerHandler.DeleteCustomer);
    }

    private static void ProductCategoryRouter(this RouteGroupBuilder r)
    {
        //INFO: NO NEED AUTHENTICATION
        r.MapGet("", ProductCategoryHandler.FindProductCategories);
        r.MapGet("/{id}", ProductCategoryHandler.FindProductCategoryById);

        //INFO: ADMIN ONLY
        r.MapPost("/admin", ProductCategoryHandler.CreateProductCategory);
        r.MapPatch("/admin/{categoryId}", ProductCategoryHandler.EditProductCategory);
        r.MapDelete("/admin/{categoryId}", ProductCategoryHandler.DeleteProductCategory);
    }
}
