using Sisal.Domain.Commom;

namespace Sisal.Domain.Entities
{
    public class Oficina : AuditableEntity
    {
        public string Nombre { get; set; } = string.Empty;
        
        public string? Descripcion { get; set; }
        
        public string Acronimo { get; set; } = string.Empty;
        
        public bool Activa { get; set; } = true;
        
        public bool EsOficinaConductores { get; set; }

        public int? JefeId { get; set; }
        
        public Empleado? Jefe { get; set; }

        
        public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
    }
}
