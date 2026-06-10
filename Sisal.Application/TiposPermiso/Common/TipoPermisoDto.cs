namespace Sisal.Application.TiposPermiso.Common
{
    public record TipoPermisoDto(
        int Id,
        string Nombre,
        string Descripcion,
        int VecesPorMes,
        bool RequiereAnexoSalida,
        bool RequiereAnexoRetorno,
        bool Activo
    );
}
