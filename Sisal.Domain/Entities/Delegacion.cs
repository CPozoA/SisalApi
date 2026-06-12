using Sisal.Domain.Commom;

namespace Sisal.Domain.Entities
{
    public class Delegacion : AuditableEntity
    {
        public int TitularId { get; set; }

        public Empleado Titular { get; set; } = null!;

        public int DelegadoId { get; set; }

        public Empleado Delegado { get; set; } = null!;

        public DateOnly FechaInicio { get; set; }

        public DateOnly FechaFin { get; set; }

        public bool Activa { get; set; }
    }
}
