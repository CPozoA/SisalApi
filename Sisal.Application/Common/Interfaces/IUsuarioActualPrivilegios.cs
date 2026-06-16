using Sisal.Domain.Enums;

namespace Sisal.Application.Common.Interfaces
{
    public interface IUsuarioActualPrivilegios
    {
        Task<bool> EsAdministradorAsync(CancellationToken cancellationToken = default);

        // Con override de admin: para acciones de gestión.
        Task<bool> TienePrivilegioAsync(TipoPrivilegio privilegio, CancellationToken cancellationToken = default);

        // Exacto, SIN override de admin: para acciones operativas del flujo.
        Task<bool> TienePrivilegioExactoAsync(TipoPrivilegio privilegio, CancellationToken cancellationToken = default);
    }
}
