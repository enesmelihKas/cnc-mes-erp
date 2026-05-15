using System.Security.Claims;
using CncMes.Infrastructure.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CncMes.App.Authentication;

public class CncMesAuthStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly IServiceProvider _services;
    private AuthenticationState _current = Anonymous;

    public CncMesAuthStateProvider(IServiceProvider services)
    {
        _services = services;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(_current);

    public async Task<SignInResultDto> SignInAsync(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            return SignInResultDto.Fail("Kullanıcı adı ve şifre zorunlu.");
        }

        using var scope = _services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var user = await userManager.FindByNameAsync(userName);
        if (user is null)
        {
            return SignInResultDto.Fail("Kullanıcı bulunamadı.");
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            return SignInResultDto.Fail("Şifre hatalı.");
        }

        var roles = await userManager.GetRolesAsync(user);

        var identity = new ClaimsIdentity(
            authenticationType: "CncMesIdentity",
            nameType: ClaimTypes.Name,
            roleType: ClaimTypes.Role);

        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName ?? userName));
        if (!string.IsNullOrEmpty(user.FullName))
        {
            identity.AddClaim(new Claim("FullName", user.FullName));
        }
        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        _current = new AuthenticationState(new ClaimsPrincipal(identity));
        NotifyAuthenticationStateChanged(Task.FromResult(_current));

        return SignInResultDto.Ok(roles.FirstOrDefault());
    }

    public void SignOut()
    {
        _current = Anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(_current));
    }
}

public record SignInResultDto(bool Succeeded, string? Error, string? Role)
{
    public static SignInResultDto Ok(string? role) => new(true, null, role);
    public static SignInResultDto Fail(string error) => new(false, error, null);
}
