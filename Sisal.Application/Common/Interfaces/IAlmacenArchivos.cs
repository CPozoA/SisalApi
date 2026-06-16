namespace Sisal.Application.Common.Interfaces
{
    public interface IAlmacenArchivos
    {
        Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken cancellationToken = default);

        Task<Stream?> AbrirAsync(string rutaRelativa, CancellationToken cancellationToken = default);

        Task EliminarAsync(string rutaRelativa, CancellationToken cancellationToken = default);
    }
}
