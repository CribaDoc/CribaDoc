using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CribaDoc.Server.Auth
{
    public class ProjectTokenService
    {
        private readonly string _secretKey;

        public ProjectTokenService(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Falta Jwt:Key en la configuración.");
        }

        public string CreateToken(long projectId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("projectId", projectId.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(descriptor);
            return tokenHandler.WriteToken(token);
        }

        public long? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var projectIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "projectId");

                if (projectIdClaim == null)
                    return null;

                if (!long.TryParse(projectIdClaim.Value, out var projectId))
                    return null;

                return projectId;
            }
            catch
            {
                return null;
            }
        }
    }
}