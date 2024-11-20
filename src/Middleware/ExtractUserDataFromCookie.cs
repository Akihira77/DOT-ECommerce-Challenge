namespace ECommerce.Middleware;

using System.Security.Claims;
using ECommerce.Service;
using ECommerce.Types;
using ECommerce.Util;

public class ExtractUserDataFromCookie
{
    private readonly RequestDelegate next;

    public ExtractUserDataFromCookie(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(
        HttpContext httpCtx,
        ICustomerService customerSvc,
        JwtService jwtSvc)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var token = httpCtx.Request.Cookies["token"];
            if (!string.IsNullOrEmpty(token))
            {
                var principal = jwtSvc.VerifyJwtToken(token);
                if (principal is not null)
                {
                    // Extract user identifier from token claims (e.g., user's email)
                    var userEmail = principal.FindFirst(ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        var customer = await customerSvc.FindCustomerByNameOrEmail(cts.Token, userEmail, false);
                        if (customer is not null)
                        {
                            // Store user data in HttpContext.Items to use in the next requests
                            httpCtx.Items["current_user"] = customer.ToCustomerOverviewDTO();
                        }
                    }
                }
            }

        }
        catch (System.Exception err)
        {
            Console.WriteLine("Extracting User Data from cookie error", err);
        }

        await this.next(httpCtx);
    }
}

