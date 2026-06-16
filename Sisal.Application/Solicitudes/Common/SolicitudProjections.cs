using Sisal.Domain.Entities;
using System.Linq.Expressions;

namespace Sisal.Application.Solicitudes.Common
{
    public static class SolicitudProjections
    {
        public static readonly Expression<Func<SolicitudSalida, SolicitudSalidaDto>> ToDto = s => new SolicitudSalidaDto(
            s.Id,
            s.EmpleadoId, s.Empleado.ApellidoPaterno + " " + s.Empleado.ApellidoMaterno + " " + s.Empleado.Nombres,
            s.TipoPermisoId, s.TipoPermiso.Nombre,
            s.Fecha, s.Motivo,
            s.VehiculoId, s.Vehiculo != null ? s.Vehiculo.Placa : null,
            s.Estado, s.HoraSalidaRealUtc, s.HoraRetornoRealUtc, s.FueraDePlazo);
    }
}
