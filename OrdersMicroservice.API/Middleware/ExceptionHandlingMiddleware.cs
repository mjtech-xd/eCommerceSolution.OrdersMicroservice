namespace OrdersMicroservice.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            //Log the exception type and message
            logger.LogError($"{ex.GetType().ToString()}:{ex.Message}");
            if(ex.InnerException is not null)
            {
                logger.LogError($"Inner Exception: {ex.InnerException.GetType().ToString()}:{ex.InnerException.Message}");
            }

            httpContext.Response.StatusCode = 500; // Internal Server Error
            await httpContext.Response.WriteAsJsonAsync(new {Message = ex.Message, Type = ex.GetType().ToString()});
        }
    }
}
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}