using Sisal.Domain.Commom;

namespace Sisal.Domain.Entities
{
    public class Vehiculo : AuditableEntity
    {
        public string Placa { get; set; } = string.Empty;   // única

        public string Marca { get; set; } = string.Empty;

        public string Modelo { get; set; } = string.Empty;

        public string? Color { get; set; }

        public bool Activo { get; set; }
    }
}
