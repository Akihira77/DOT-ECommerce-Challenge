namespace ECommerce.Types;

public abstract class ApiError
{
    public string Type { get; }
    public string Title { get; }
    public string Detail { get; }
    public int StatusCode { get; }

    protected ApiError(string type, string title, string detail, int statusCode)
    {
        Type = type;
        Title = title;
        Detail = detail;
        StatusCode = statusCode;
    }

    public IResult ToResult()
    {
        return Results.Problem(
            type: Type,
            title: Title,
            detail: Detail,
            statusCode: StatusCode
        );
    }
}

public class NotFoundError : ApiError
{
    public NotFoundError(string detail)
        : base(type: nameof(NotFoundError),
               title: "Resource Not Found",
               detail: detail,
               statusCode: StatusCodes.Status404NotFound)
    {
    }
}

public class BadRequestError : ApiError
{
    public BadRequestError(string detail)
        : base(type: nameof(BadRequestError),
               title: "Bad Request",
               detail: detail,
               statusCode: StatusCodes.Status400BadRequest)
    {
    }
}

public class UnauthorizedError : ApiError
{
    public UnauthorizedError(string detail)
        : base(type: nameof(UnauthorizedError),
               title: "Unauthorized",
               detail: detail,
               statusCode: StatusCodes.Status401Unauthorized)
    {
    }
}

public class ForbiddenError : ApiError
{
    public ForbiddenError(string detail)
        : base(type: nameof(ForbiddenError),
               title: "Forbidden",
               detail: detail,
               statusCode: StatusCodes.Status403Forbidden)
    {
    }
}


public class InternalServerError : ApiError
{
    public InternalServerError(string detail)
        : base(type: nameof(InternalServerError),
               title: "Server Error",
               detail: detail,
               statusCode: StatusCodes.Status500InternalServerError)
    {
    }
}

