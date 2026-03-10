using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CribaDoc.Server.Auth
{
    public class AppTokenService
    {
        private readonly string _secretKey;

        public AppTokenService(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Falta Jwt:Key en la configuración.");
        }

        public string CreateUserToken(long userId, string nombreUsuario)
        {
            return CreateToken(new[]
            {
                new Claim("scope", "user"),
                new Claim("userId", userId.ToString()),
                new Claim("nombreUsuario", nombreUsuario)
            });
        }

        public string CreateProjectToken(long userId, long projectId)
        {
            return CreateToken(new[]
            {
                new Claim("scope", "project"),
                new Claim("userId", userId.ToString()),
                new Claim("projectId", projectId.ToString())
            });
        }

        private string CreateToken(IEnumerable<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(descriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}