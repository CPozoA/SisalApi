using Sisal.Domain.Entities;
using System.Linq.Expressions;

namespace Sisal.Application.Delegaciones.Common
{
    public static class DelegacionProjections
    {
        public static readonly Expression<Func<Delegacion, DelegacionDto>> ToDto = d => new DelegacionDto(
            d.Id,
            d.TitularId, d.Titular.ApellidoPaterno + " " + d.Titular.ApellidoMaterno + " " + d.Titular.Nombres,
            d.DelegadoId, d.Delegado.ApellidoPaterno + " " + d.Delegado.ApellidoMaterno + " " + d.Delegado.Nombres,
            d.FechaInicio, d.FechaFin, d.Activa);
    }
}
