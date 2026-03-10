using System.Security.Claims;

namespace CribaDoc.Server.Auth
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetScope(this ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(x => x.Type == "scope")?.Value;
        }

        public static long? GetUserId(this ClaimsPrincipal user)
        {
            var value = user.Claims.FirstOrDefault(x => x.Type == "userId")?.Value;

            if (long.TryParse(value, out var userId))
                return userId;

            return null;
        }

        public static long? GetProjectId(this ClaimsPrincipal user)
        {
            var value = user.Claims.FirstOrDefault(x => x.Type == "projectId")?.Value;

            if (long.TryParse(value, out var projectId))
                return projectId;

            return null;
        }
    }
}
