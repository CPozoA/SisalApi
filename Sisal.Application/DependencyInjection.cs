using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;
using Sisal.Application.Common.Services;
using Sisal.Application.Solicitudes.Common;
using System.Reflection;

namespace Sisal.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddScoped<IDispatcher, Dispatcher>();
            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

            var openHandlers = new[] { typeof(ICommandHandler<,>), typeof(IQueryHandler<,>) };
            foreach (var type in assembly.GetTypes().Where(t => t is { IsAbstract: false, IsInterface: false }))
            {
                foreach (var contract in type.GetInterfaces()
                             .Where(i => i.IsGenericType && openHandlers.Contains(i.GetGenericTypeDefinition())))
                {
                    services.AddScoped(contract, type);
                }
            }

            services.AddScoped<IUsuarioActualPrivilegios, UsuarioActualPrivilegios>();

            services.AddScoped<IVisibilidadSolicitud, VisibilidadSolicitud>();

            return services;
        }
    }
}
