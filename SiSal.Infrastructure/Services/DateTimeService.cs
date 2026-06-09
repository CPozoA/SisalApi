using Sisal.Application.Common.Interfaces;

namespace SiSal.Infrastructure.Services
{
    public class DateTimeService : IDateTime
    {
        // .NET 10 resuelve identificadores IANA en Windows y Linux via ICU.
        private static readonly TimeZoneInfo ZonaLima = TimeZoneInfo.FindSystemTimeZoneById("America/Lima");

        public DateTime UtcNow => DateTime.UtcNow;

        public DateTimeOffset NowEnLima => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ZonaLima);
    }
}
