using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Solicitudes.Common;
using Sisal.Domain.Enums;
using SiSal.Infrastructure.Persistence;

namespace SiSal.Infrastructure.BackgroundServices
{
    public sealed class CaducidadBackgroundService(
        IServiceScopeFactory scopeFactory,
        IDateTime clock,
        ILogger<CaducidadBackgroundService> logger) : BackgroundService
    {
        private static readonly TimeSpan Intervalo = TimeSpan.FromHours(1);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Corre al arrancar (pone al día lo que haya quedado) y luego cada hora.
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<SisalDbContext>();

                    await CerrarVencidasAsync(db, stoppingToken);
                    await LimpiarRefreshTokensAsync(db, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;   // apagado normal de la app
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error en la barrida de caducidad.");
                }

                try { await Task.Delay(Intervalo, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task CerrarVencidasAsync(SisalDbContext db, CancellationToken cancellationToken)
        {
            var hoy = DateOnly.FromDateTime(clock.NowEnLima.DateTime);
            var ahora = clock.UtcNow;

            // Todo lo que sigue "vivo" pero su día ya pasó.
            var vencidas = await db.SolicitudesSalida
                .Where(s => s.Fecha < hoy && EstadosSolicitud.Activos.Contains(s.Estado))
                .ToListAsync(cancellationToken);

            if (vencidas.Count == 0) return;

            foreach (var s in vencidas)
            {
                var nuncaSalio = s.Estado is EstadoSolicitud.PendienteJefe
                    or EstadoSolicitud.PendienteRrhh
                    or EstadoSolicitud.ListoParaSalir;

                if (nuncaSalio)
                {
                    // No llegó a usarse: caduca y el cupo se devuelve solo (Caducado no cuenta en cuota).
                    s.Transicionar(EstadoSolicitud.Caducado, null,
                        "Caducó: no se utilizó antes del fin del día.", ahora);
                }
                else
                {
                    // FueraDeLaInstitucion o PendienteAnexoRetorno: no cerró el trámite ese día.
                    s.FueraDePlazo = true;
                    s.Transicionar(EstadoSolicitud.FueraDePlazo, null,
                        "Fuera de plazo: no se cerró antes del fin del día.", ahora);
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Caducidad: se cerraron {Cuantas} solicitudes vencidas.", vencidas.Count);
        }

        private async Task LimpiarRefreshTokensAsync(SisalDbContext db, CancellationToken cancellationToken)
        {
            // Conserva ~30 días para rastreo; borra los que expiraron hace más de eso.
            var limite = clock.UtcNow.AddDays(-30);

            var borrados = await db.RefreshTokens
                .Where(t => t.ExpiraEnUtc < limite)
                .ExecuteDeleteAsync(cancellationToken);

            if (borrados > 0)
                logger.LogInformation("Limpieza: se eliminaron {Cuantos} refresh tokens antiguos.", borrados);
        }
    }
}
