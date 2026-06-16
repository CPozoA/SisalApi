using Sisal.Domain.Enums;

namespace Sisal.Application.Solicitudes.Common
{
    public record SolicitudSalidaDto(
        int Id,
        int EmpleadoId, string Empleado,
        int TipoPermisoId, string TipoPermiso,
        DateOnly Fecha,
        string? Motivo,
        int? VehiculoId, string? Vehiculo,
        EstadoSolicitud Estado,
        DateTime? HoraSalidaRealUtc,
        DateTime? HoraRetornoRealUtc,
        bool FueraDePlazo);
}
