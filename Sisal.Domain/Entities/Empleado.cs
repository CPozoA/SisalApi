using Sisal.Domain.Commom;
using Sisal.Domain.Enums;

namespace Sisal.Domain.Entities
{
    public class Empleado : AuditableEntity
    {
        public string Dni { get; set; } = string.Empty;
        
        public string ApellidoPaterno { get; set; } = string.Empty;
        
        public string ApellidoMaterno { get; set; } = string.Empty;
        
        public string Nombres { get; set; } = string.Empty;
        
        public string? Celular { get; set; }
        
        public string? Correo { get; set; }
        
        public TipoEmpleado TipoEmpleado { get; set; }

        public int OficinaId { get; set; }
        
        public Oficina Oficina { get; set; } = null!;

        public int? JefeInmediatoId { get; set; }
        
        public Empleado? JefeInmediato { get; set; }

        public string PasswordHash { get; set; } = string.Empty;
        
        public bool DebeCambiarClave { get; set; } = true;
        
        public bool Activo { get; set; } = true;

        public ICollection<AsignacionPrivilegio> Privilegios { get; set; } = new List<AsignacionPrivilegio>();
    }
}
