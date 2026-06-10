namespace Sisal.Application.Oficinas.Commons
{
    public record OficinaDto(
        int Id,
        string Nombre,
        string Descripcion,
        string Acronimo,
        bool Activa,
        bool EsOficinaConductores
    );
}
