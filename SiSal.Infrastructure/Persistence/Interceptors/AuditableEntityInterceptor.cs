using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sisal.Application.Common.Interfaces;
using Sisal.Domain.Commom;

namespace SiSal.Infrastructure.Persistence.Interceptors
{
    public sealed class AuditableEntityInterceptor(ICurrentUser currentUser, IDateTime clock)
        : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData, InterceptionResult<int> result)
        {
            AplicarAuditoria(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            AplicarAuditoria(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void AplicarAuditoria(DbContext? context)
        {
            if (context is null) return;

            var ahora = clock.UtcNow;
            var usuarioId = currentUser.EmpleadoId;

            foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAtUtc = ahora;
                    entry.Entity.CreatedBy = usuarioId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedAtUtc = ahora;
                    entry.Entity.ModifiedBy = usuarioId;
                }
            }
        }
    }
}
