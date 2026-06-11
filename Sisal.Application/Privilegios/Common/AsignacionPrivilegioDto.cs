using Sisal.Domain.Enums;

namespace Sisal.Application.Privilegios.Common
{
    public record AsignacionPrivilegioDto(
        int Id,
        int EmpleadoId,
        TipoPrivilegio Privilegio,
        int? OficinaId,
        string? Oficina,
        bool Activo);
}
