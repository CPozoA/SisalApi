namespace Sisal.Application.Delegaciones.Common
{
    public record DelegacionDto(
        int Id,
        int TitularId, string Titular,
        int DelegadoId, string Delegado,
        DateOnly FechaInicio, DateOnly FechaFin,
        bool Activa);
}
