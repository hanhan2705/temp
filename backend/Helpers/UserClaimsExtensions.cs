using System.Security.Claims;

namespace backend.Helpers
{
    public static class UserClaimsExtensions
    {
        public static long GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.Parse(id ?? "0");
        }

        public static string GetUserRole(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Role) ?? "";
        }
    }
}
