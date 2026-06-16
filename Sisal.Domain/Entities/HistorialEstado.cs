using Sisal.Domain.Commom;
using Sisal.Domain.Enums;

namespace Sisal.Domain.Entities
{
    public class HistorialEstado : BaseEntity
    {
        public int SolicitudSalidaId { get; set; }

        public SolicitudSalida Solicitud { get; set; } = null!;

        public EstadoSolicitud Estado { get; set; }     // el estado al que se pasó

        public int? RegistradoPorId { get; set; }        // quién hizo la transición

        public DateTime FechaUtc { get; set; }

        public string? Comentario { get; set; }

        public Empleado? RegistradoPor { get; set; }
    }
}
