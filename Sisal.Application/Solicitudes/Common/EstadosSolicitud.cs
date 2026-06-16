using Sisal.Domain.Enums;

namespace Sisal.Application.Solicitudes.Common
{
    public static class EstadosSolicitud
    {
        // Estados "en curso": una solicitud en cualquiera de estos cuenta como activa.
        public static readonly EstadoSolicitud[] Activos =
        [
            EstadoSolicitud.PendienteJefe,
            EstadoSolicitud.PendienteRrhh,
            EstadoSolicitud.ListoParaSalir,
            EstadoSolicitud.FueraDeLaInstitucion,
            EstadoSolicitud.PendienteAnexoRetorno
        ];

        // Estados que NO consumen cuota (se devuelve el cupo).
        public static readonly EstadoSolicitud[] NoCuentanEnCuota =
        [
            EstadoSolicitud.RechazadoPorJefe,
            EstadoSolicitud.RechazadoPorRrhh,
            EstadoSolicitud.Cancelado,
            EstadoSolicitud.Caducado
        ];
    }
}
