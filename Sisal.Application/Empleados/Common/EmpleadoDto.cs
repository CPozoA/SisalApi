using Sisal.Domain.Enums;

namespace Sisal.Application.Empleados.Common
{
    public record EmpleadoDto(
        int Id,
        string Dni,
        string ApellidoPaterno,
        string ApellidoMaterno,
        string Nombres,
        string? Celular,
        string? Correo,
        TipoEmpleado TipoEmpleado,
        int OficinaId,
        string Oficina,
        int? JefeInmediatoId,
        string? JefeInmediato,
        bool DebeCambiarClave,
        bool Activo
    );

}
