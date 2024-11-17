using ECommerce.Types;

namespace ECommerce.Middleware;


public static class AuthenticationValidationMiddleware
{
    public static EndpointFilterDelegate RequireAuthentication(EndpointFilterFactoryContext factoryCtx, EndpointFilterDelegate next)
    {
        return async invocationContext =>
        {
            var current_user = invocationContext.HttpContext.Items["current_user"] as Customer;
            if (current_user is not null)
            {
                return await next(invocationContext);
            }

            Console.WriteLine($"Customer is not authenticated");
            return Results.Unauthorized();
        };
    }

    public static EndpointFilterDelegate RequireAdmin(EndpointFilterFactoryContext factoryCtx, EndpointFilterDelegate next)
    {
        return async invocationContext =>
        {
            var current_user = invocationContext.HttpContext.Items["current_user"] as Customer;
            if (current_user is not null && current_user.Role.Equals(UserRoles.ADMIN))
            {
                return await next(invocationContext);
            }

            Console.WriteLine($"Customer is not an ADMIN {current_user}");
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        };
    }

}
