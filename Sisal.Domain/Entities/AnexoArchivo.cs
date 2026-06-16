using Sisal.Domain.Commom;
using Sisal.Domain.Enums;

namespace Sisal.Domain.Entities
{
    public class AnexoArchivo : AuditableEntity
    {
        public int SolicitudSalidaId { get; set; }

        public SolicitudSalida Solicitud { get; set; } = null!;

        public TipoAnexo Tipo { get; set; }                  // Salida (previo) o Retorno

        public string NombreOriginal { get; set; } = string.Empty;

        public string RutaArchivo { get; set; } = string.Empty;  // ruta relativa en disco

        public string ContentType { get; set; } = string.Empty;

        public long TamanoBytes { get; set; }
    }
}
