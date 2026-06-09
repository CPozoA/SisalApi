using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SiSal.API.Middleware
{
    public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Excepcion no controlada: {Message}", exception.Message);

            var (status, title) = exception switch
            {
                ValidationException => (StatusCodes.Status400BadRequest, "Error de validacion"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "No autenticado"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
            };

            var detail = exception switch
            {
                ValidationException validation => string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                UnauthorizedAccessException => exception.Message,
                KeyNotFoundException => exception.Message,
                _ => null   // En errores 500 no exponemos el mensaje interno
            };

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            };

            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }
    }
}
