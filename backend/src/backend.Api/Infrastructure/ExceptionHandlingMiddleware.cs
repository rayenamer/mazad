using Backend.Api.Domain;

namespace Backend.Api.Infrastructure;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Laisse passer la requête vers le contrôleur.
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await EcrireErreurAsync(context, 400, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await EcrireErreurAsync(context, 404, ex.Message);
        }
        catch (ConflictException ex)
        {
            await EcrireErreurAsync(context, 409, ex.Message);
        }
    }

    private static async Task EcrireErreurAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new { statusCode, message });
    }
}