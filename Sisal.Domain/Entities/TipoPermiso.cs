using Sisal.Domain.Commom;

namespace Sisal.Domain.Entities
{
    public class TipoPermiso : AuditableEntity
    {
        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;
        
        public int VecesPorMes { get; set; }
        
        public bool RequiereAnexoSalida { get; set; }
        
        public bool RequiereAnexoRetorno { get; set; }
        
        public bool Activo { get; set; }
    }
}
