using Microsoft.AspNetCore.Identity;
using Sisal.Application.Common.Interfaces;
using Sisal.Domain.Entities;

namespace SiSal.Infrastructure.Identity
{
    public class PasswordHasherService : IPasswordHasher
    {
        private readonly PasswordHasher<Empleado> _hasher = new();

        public string Hash(string password) {
            return _hasher.HashPassword(null!, password);
        }

        public bool Verify(string hash, string password)
        {
            return _hasher.VerifyHashedPassword(null!, hash, password) != PasswordVerificationResult.Failed;
        }

    }
}
