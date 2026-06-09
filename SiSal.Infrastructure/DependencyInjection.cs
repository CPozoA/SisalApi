using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Models;
using Sisal.Application.Identity;
using SiSal.Infrastructure.Identity;
using SiSal.Infrastructure.Persistence;
using SiSal.Infrastructure.Persistence.Interceptors;
using SiSal.Infrastructure.Services;

namespace SiSal.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("SisalDb")
                ?? throw new InvalidOperationException("No se configuro la cadena de conexion 'SisalDb'.");

            services.AddScoped<AuditableEntityInterceptor>();

            services.AddDbContext<SisalDbContext>((sp, options) =>
                options.UseSqlServer(connectionString, sql =>
                    sql.MigrationsAssembly(typeof(SisalDbContext).Assembly.FullName))
                   .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<SisalDbContext>());

            services.AddScoped<SisalDbInitializer>();

            services.AddSingleton<IDateTime, DateTimeService>();
            services.AddSingleton<IPasswordHasher, PasswordHasherService>();

            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

            return services;
        }
    }
}
