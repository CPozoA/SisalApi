using Sisal.Domain.Commom;
using Sisal.Domain.Entities;

namespace SiSal.Infrastructure.Identity
{
    public class RefreshToken : BaseEntity
    {
        public int EmpleadoId { get; set; }
        
        public Empleado Empleado { get; set; } = null!;
        
        public string Token { get; set; } = string.Empty;
        
        public DateTime ExpiraEnUtc { get; set; }
        
        public DateTime CreadoEnUtc { get; set; }
        
        public DateTime? RevocadoEnUtc { get; set; }
        
        public string? ReemplazadoPorToken { get; set; }
    }
}
