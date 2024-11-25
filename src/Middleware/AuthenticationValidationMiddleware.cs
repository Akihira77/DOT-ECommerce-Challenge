using ECommerce.Types;

namespace ECommerce.Middleware;


public static class AuthenticationValidationMiddleware
{
    public static EndpointFilterDelegate RequireAuthentication(
        EndpointFilterFactoryContext factoryCtx,
        EndpointFilterDelegate next)
    {
        return async invocationContext =>
        {
            var current_user = invocationContext.HttpContext.Items["current_user"] as CustomerOverviewDTO;
            if (current_user is not null)
            {
                return await next(invocationContext);
            }

            Console.WriteLine($"Customer is not authenticated");
            return new UnauthorizedError("Sign-in first").ToResult();
        };
    }

    public static EndpointFilterDelegate RequireAdmin(
        EndpointFilterFactoryContext factoryCtx,
        EndpointFilterDelegate next)
    {
        return async invocationContext =>
        {
            var current_user = invocationContext.HttpContext.Items["current_user"] as CustomerOverviewDTO;
            if (current_user is not null && current_user.role.Equals(UserRoles.ADMIN))
            {
                return await next(invocationContext);
            }

            Console.WriteLine($"Customer is not an ADMIN {current_user}");
            return new ForbiddenError("You dont have the right permission for this feature").ToResult();
        };
    }

}
