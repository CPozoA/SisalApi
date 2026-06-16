using Sisal.Domain.Commom;
using Sisal.Domain.Enums;

namespace Sisal.Domain.Entities
{
    public class SolicitudSalida : AuditableEntity
    {
        public int EmpleadoId { get; set; }

        public Empleado Empleado { get; set; } = null!;

        public int TipoPermisoId { get; set; }

        public TipoPermiso TipoPermiso { get; set; } = null!;

        public DateOnly Fecha { get; set; }            // siempre HOY (hora de Lima)

        public string? Motivo { get; set; }

        public int? VehiculoId { get; set; }            // si sale con vehículo

        public Vehiculo? Vehiculo { get; set; }

        public EstadoSolicitud Estado { get; set; }

        public DateTime? HoraSalidaRealUtc { get; set; }   // las registra vigilancia

        public DateTime? HoraRetornoRealUtc { get; set; }

        public bool FueraDePlazo { get; set; }

        public ICollection<HistorialEstado> Historial { get; set; } = new List<HistorialEstado>();
    }
}
