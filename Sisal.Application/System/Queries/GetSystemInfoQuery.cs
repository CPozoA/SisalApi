using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Messaging;

namespace Sisal.Application.System.Queries
{
    public sealed record SystemInfoDto(string Nombre, string Version, DateTimeOffset HoraLima);

    public sealed record GetSystemInfoQuery : IQuery<SystemInfoDto>;

    public sealed class GetSystemInfoQueryHandler(IDateTime clock)
        : IQueryHandler<GetSystemInfoQuery, SystemInfoDto>
    {
        public Task<SystemInfoDto> Handle(GetSystemInfoQuery query, CancellationToken cancellationToken)
            => Task.FromResult(new SystemInfoDto("SISAL", "0.1.0-fase0", clock.NowEnLima));
    }
}
