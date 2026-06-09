using Sisal.Domain.Commom;
using Sisal.Domain.Enums;

namespace Sisal.Domain.Entities
{
    public class AsignacionPrivilegio : AuditableEntity
    {
        public int EmpleadoId { get; set; }

        public Empleado Empleado { get; set; } = null!;

        public TipoPrivilegio Privilegio { get; set; }

        public int? OficinaId { get; set; }

        public Oficina? Oficina { get; set; }

        public bool Activo { get; set; } = true;
    }
}
