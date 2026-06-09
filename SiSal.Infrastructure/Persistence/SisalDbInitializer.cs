using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sisal.Application.Common.Interfaces;
using Sisal.Domain.Entities;
using Sisal.Domain.Enums;

namespace SiSal.Infrastructure.Persistence
{
    public class SisalDbInitializer(SisalDbContext context, IPasswordHasher passwordHasher, ILogger<SisalDbInitializer> logger)
    {
        public const string ClavePorDefecto = "Sisal2027!";

        public async Task InitialiseAsync()
        {
            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al aplicar las migraciones de la base de datos.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            if (await context.Oficinas.AnyAsync())
            {
                return;
            }

            var now = DateTime.UtcNow;

            var oficinaConductores = new Oficina
            {
                Nombre = "Conductores",
                Acronimo = "COND",
                Descripcion = "Oficina de conductores",
                Activa = true,
                EsOficinaConductores = true,
                CreatedAtUtc = now
            };

            var oficinaAdmin = new Oficina
            {
                Nombre = "Administracion del Sistema",
                Acronimo = "ADM",
                Descripcion = "Oficina del administrador del sistema",
                Activa = true,
                CreatedAtUtc = now
            };

            context.Oficinas.AddRange(oficinaConductores, oficinaAdmin);
            await context.SaveChangesAsync();

            var admin = new Empleado
            {
                Dni = "00000000",
                ApellidoPaterno = "Administrador",
                ApellidoMaterno = "Administrador",
                Nombres = "SISAL",
                OficinaId = oficinaAdmin.Id,
                TipoEmpleado = TipoEmpleado.F4,
                PasswordHash = passwordHasher.Hash(ClavePorDefecto),
                DebeCambiarClave = true,
                Activo = true,
                CreatedAtUtc = now
            };

            context.Empleados.Add(admin);
            await context.SaveChangesAsync();

            context.Privilegios.Add(new AsignacionPrivilegio
            {
                EmpleadoId = admin.Id,
                Privilegio = TipoPrivilegio.Administrador,
                Activo = true,
                CreatedAtUtc = now
            });
            await context.SaveChangesAsync();

            logger.LogInformation("Seed inicial completado: oficina de conductores y usuario administrador creados.");
        }
    }
}
