namespace Sisal.Application.Solicitudes.Common
{
    public record SolicitudDetalleDto(

        SolicitudSalidaDto Solicitud,

        IReadOnlyList<HistorialEntradaDto> Historial
    );
}
