using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace ThuInfoWeb
{
    public class Account
    {
        public static async Task LoginAsync(HttpContext context, string name, bool isAdmin)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name,name),
                new Claim(ClaimTypes.Role,isAdmin?"admin":"guest")
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "any"));
            await context.SignInAsync("Cookies", user, new AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddMinutes(20)
            });
        }
        public static async Task LogoutAsync(HttpContext context)
        {
            await context.SignOutAsync();
        }
    }
}
