using System.Security.Claims;
using ECommerce.Service;

public class ExtractUserDataFromCookie
{
    private readonly RequestDelegate next;

    public ExtractUserDataFromCookie(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpCtx, ICustomerService customerSvc, JwtService jwtSvc)
    {
        try
        {
            var token = httpCtx.Request.Cookies["token"];
            if (!string.IsNullOrEmpty(token))
            {
                var principal = jwtSvc.VerifyJwtToken(token);

                // Extract user identifier from token claims (e.g., user's email)
                var userEmail = principal.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var user = await customerSvc.FindCustomerByNameOrEmail(userEmail, false);
                    if (user is not null)
                    {
                        // Store user data in HttpContext.Items to use in the next requests
                        httpCtx.Items["current_user"] = user;
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

