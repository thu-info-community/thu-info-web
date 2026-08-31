using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace ThuInfoWeb;

public class UserManager(IHttpContextAccessor accessor, TimeProvider timeProvider)
{
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task DoLoginAsync(string name, bool isAdmin)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, name), new(ClaimTypes.Role, isAdmin ? "admin" : "guest") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies"));
        await accessor.HttpContext!.SignInAsync("Cookies", user,
            new AuthenticationProperties { ExpiresUtc = _timeProvider.GetUtcNow().AddMinutes(20) });
    }

    public async Task DoLogoutAsync()
    {
        await accessor.HttpContext!.SignOutAsync();
    }
}
