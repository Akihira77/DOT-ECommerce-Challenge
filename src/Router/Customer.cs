using ECommerce.Handler;
using ECommerce.Middleware;

namespace ECommerce.Router;

public static class CustomerRouter
{
    public static void RegisterCustomerRouter(this RouteGroupBuilder r)
    {
        r.UnrequireAnythingRoutes();
        r.RequireAuthenticationRoutes();
        r.RequireAdminRoutes();
    }

    private static void UnrequireAnythingRoutes(this RouteGroupBuilder r)
    {
        r.MapGet("", CustomerHandler.FindCustomers);
        r.MapGet("/id/{id}", CustomerHandler.FindCustomerById);
        r.MapGet("/name-or-email/{str}", CustomerHandler.FindCustomerByNameOrEmail);
        r.MapPost("/register", CustomerHandler.CreateCustomer);
        r.MapPost("/login", CustomerHandler.Login);
    }

    private static void RequireAuthenticationRoutes(this RouteGroupBuilder r)
    {
        var requireAuthGroup = r.MapGroup("").
            AddEndpointFilterFactory(AuthenticationValidationMiddleware.RequireAuthentication);
        requireAuthGroup.MapGet("/my-info", CustomerHandler.FindMyCustomerInfo);
        requireAuthGroup.MapPatch("", CustomerHandler.EditCustomer);
        requireAuthGroup.MapPost("/addresses", CustomerHandler.AddCustomerAddress);
    }

    private static void RequireAdminRoutes(this RouteGroupBuilder r)
    {
        var requireAdminGroup = r.MapGroup("/admin").
            AddEndpointFilterFactory(AuthenticationValidationMiddleware.RequireAdmin);
        requireAdminGroup.MapPatch("/change-roles/{customerId}", CustomerHandler.ChangeCustomerRoles);
        requireAdminGroup.MapDelete("/{customerId}", CustomerHandler.DeleteCustomer);
    }
}
