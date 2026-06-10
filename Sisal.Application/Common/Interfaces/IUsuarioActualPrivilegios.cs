using Sisal.Domain.Enums;

namespace Sisal.Application.Common.Interfaces
{
    public interface IUsuarioActualPrivilegios
    {
        Task<bool> EsAdministradorAsync(CancellationToken cancellationToken = default);

        Task<bool> TienePrivilegioAsync(TipoPrivilegio privilegio, CancellationToken cancellationToken = default);
    }
}
