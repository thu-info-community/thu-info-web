using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace ThuInfoWeb
{
    public class UserManager
    {
        private readonly IHttpContextAccessor _accessor;

        public UserManager(IHttpContextAccessor accessor)
        {
            this._accessor = accessor;
        }
        public async Task DoLoginAsync(string name, bool isAdmin)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name,name),
                new Claim(ClaimTypes.Role,isAdmin?"admin":"guest")
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "any"));
            await _accessor.HttpContext.SignInAsync("Cookies", user, new AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddMinutes(20)
            });
        }
        public async Task DoLogoutAsync()
        {
            await _accessor.HttpContext.SignOutAsync();
        }
    }
}
