namespace Sisal.Application.Common.Interfaces
{
    public interface IDateTime
    {
        DateTime UtcNow { get; }

        DateTimeOffset NowEnLima { get; }
    }
}
