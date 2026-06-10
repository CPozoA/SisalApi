using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Sisal.Application.Common.Messaging
{
    /// <summary>
    /// Despachador ligero de comandos y consultas. Resuelve el handler por DI y
    /// ejecuta la validacion (FluentValidation) si existe un validador registrado.
    /// Reemplaza a MediatR sin dependencias de licencia comercial.
    /// </summary>
    public sealed class Dispatcher(IServiceProvider provider) : IDispatcher
    {
        public Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
            => InvokeAsync<TResult>(command, typeof(ICommandHandler<,>), cancellationToken);

        public Task<TResult> Query<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
            => InvokeAsync<TResult>(query, typeof(IQueryHandler<,>), cancellationToken);

        private async Task<TResult> InvokeAsync<TResult>(object request, Type openHandlerType, CancellationToken cancellationToken)
        {
            var requestType = request.GetType();

            var validatorType = typeof(IValidator<>).MakeGenericType(requestType);
            if (provider.GetService(validatorType) is IValidator validator)
            {
                var context = new ValidationContext<object>(request);
                var validation = await validator.ValidateAsync(context, cancellationToken);
                if (!validation.IsValid)
                {
                    throw new ValidationException(validation.Errors);
                }
            }

            var handlerType = openHandlerType.MakeGenericType(requestType, typeof(TResult));
            var handler = provider.GetRequiredService(handlerType);
            var method = handlerType.GetMethod("Handle")
                ?? throw new InvalidOperationException($"No se encontro el metodo Handle en {handlerType.Name}.");

            return await (Task<TResult>)method.Invoke(handler, [request, cancellationToken])!;
        }

    }
}