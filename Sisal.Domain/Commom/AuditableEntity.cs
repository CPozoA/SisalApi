namespace Sisal.Domain.Commom
{
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime CreatedAtUtc { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedAtUtc { get; set; }

        public int? ModifiedBy { get; set; }

    }
}
