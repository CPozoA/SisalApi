namespace Sisal.Application.Common.Interfaces
{
    public interface ICurrentUser
    {
        int? EmpleadoId { get; }

        string? Dni { get; }

        bool IsAuthenticated { get; }
    }
}
