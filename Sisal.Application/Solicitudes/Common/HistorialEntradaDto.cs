using Sisal.Domain.Enums;

namespace Sisal.Application.Solicitudes.Common
{
    public record HistorialEntradaDto(
        EstadoSolicitud Estado, string? RegistradoPor, DateTime FechaUtc, string? Comentario
    );
}
