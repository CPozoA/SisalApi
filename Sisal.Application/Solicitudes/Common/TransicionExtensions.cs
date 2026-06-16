using Sisal.Domain.Entities;
using Sisal.Domain.Enums;

namespace Sisal.Application.Solicitudes.Common
{
    public static class TransicionExtensions
    {
        public static void Transicionar(this SolicitudSalida solicitud,
            EstadoSolicitud nuevo, int? actorId, string? comentario, DateTime ahoraUtc)
        {
            solicitud.Estado = nuevo;
            solicitud.Historial.Add(new HistorialEstado
            {
                Estado = nuevo,
                RegistradoPorId = actorId,
                FechaUtc = ahoraUtc,
                Comentario = comentario
            });
        }
    }
}
