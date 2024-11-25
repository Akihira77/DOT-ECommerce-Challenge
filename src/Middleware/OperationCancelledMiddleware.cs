namespace ECommerce.Middleware;

public class OperationCanceledMiddleware
{
    private readonly RequestDelegate next;
    public OperationCanceledMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this.next(context);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Request was cancelled");
            context.Response.StatusCode = 409;
        }
    }
}

