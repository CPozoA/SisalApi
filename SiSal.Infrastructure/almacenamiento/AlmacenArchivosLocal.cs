using Microsoft.Extensions.Configuration;
using Sisal.Application.Common.Interfaces;

namespace SiSal.Infrastructure.almacenamiento
{
    public sealed class AlmacenArchivosLocal : IAlmacenArchivos
    {
        private readonly string _rutaBase;

        public AlmacenArchivosLocal(IConfiguration config)
        {
            _rutaBase = config["Anexos:RutaBase"] ?? "anexos";
            Directory.CreateDirectory(_rutaBase);
        }

        public async Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken cancellationToken = default)
        {
            var nombre = $"{Guid.NewGuid():N}{extension}";
            var rutaCompleta = Path.Combine(_rutaBase, nombre);
            await using var destino = File.Create(rutaCompleta);
            await contenido.CopyToAsync(destino, cancellationToken);
            return nombre;
        }

        public Task<Stream?> AbrirAsync(string rutaRelativa, CancellationToken cancellationToken = default)
        {
            var ruta = Path.Combine(_rutaBase, rutaRelativa);
            return Task.FromResult(File.Exists(ruta) ? File.OpenRead(ruta) : (Stream?)null);
        }

        public Task EliminarAsync(string rutaRelativa, CancellationToken cancellationToken = default)
        {
            var ruta = Path.Combine(_rutaBase, rutaRelativa);
            if (File.Exists(ruta)) File.Delete(ruta);
            return Task.CompletedTask;
        }
    }
}
