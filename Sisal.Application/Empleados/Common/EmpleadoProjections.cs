using Sisal.Domain.Entities;
using System.Linq.Expressions;

namespace Sisal.Application.Empleados.Common
{
    public static class EmpleadoProjections
    {
        public static readonly Expression<Func<Empleado, EmpleadoDto>> ToDto = e => new EmpleadoDto(
            e.Id, e.Dni, e.ApellidoPaterno, e.ApellidoMaterno, e.Nombres,
            e.Celular, e.Correo, e.TipoEmpleado,
            e.OficinaId, e.Oficina.Nombre,
            e.JefeInmediatoId,
            e.JefeInmediato != null
                ? e.JefeInmediato.ApellidoPaterno + " " + e.JefeInmediato.ApellidoMaterno + " " + e.JefeInmediato.Nombres
                : null,
            e.DebeCambiarClave, e.Activo);
    }
}