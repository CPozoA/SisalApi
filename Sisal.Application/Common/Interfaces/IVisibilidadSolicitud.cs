namespace Sisal.Application.Common.Interfaces
{
    public interface IVisibilidadSolicitud
    {
        Task<bool> PuedeVerAsync(int solicitudId, CancellationToken cancellationToken = default);
    }
}
