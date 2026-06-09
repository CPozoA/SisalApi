using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Models;
using Sisal.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Sisal.Application.Identity
{
    public class JwtTokenGenerator(IOptions<JwtSettings> options, IDateTime clock) : IJwtTokenGenerator
    {
        private readonly JwtSettings _settings = options.Value;

        public string GenerateAccessToken(Empleado empleado)
        {
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, empleado.Id.ToString()),
            new("dni", empleado.Dni),
            new(JwtRegisteredClaimNames.Name, $"{empleado.Nombres} {empleado.ApellidoPaterno}"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: clock.UtcNow.AddMinutes(_settings.ExpiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshTokenInfo GenerateRefreshToken()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var expira = clock.UtcNow.AddMinutes(_settings.RefreshTokenExpiryMinutes);
            return new RefreshTokenInfo(token, expira);
        }
    }
}
