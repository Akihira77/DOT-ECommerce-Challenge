using ECommerce.Handler;

namespace ECommerce;

public static class Router
{
    private readonly static string BASE_PATH = "/api";
    public static void MainRouter(this WebApplication app)
    {
        var api = app.MapGroup(BASE_PATH);
        var customerGroup = api.MapGroup("/customers");

        customerGroup.CustomerRouter();
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
}
