using System.Reflection;
using System.Security.Claims;
using AIVES.BLL.Interfaces;
using AIVES.BLL.Models;
using AIVES.BLL.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using StudentNameMVC.Controllers;
using StudentNameMVC.Security;

namespace AIVES.Tests;

public sealed class AuthenticationPresentationTests
{
    [Fact]
    public void Claims_factory_creates_required_identity_claims()
    {
        var principal = AuthenticationClaimsFactory.CreatePrincipal(new AuthenticatedUser(
            "17",
            "Staff Member",
            "staff@example.test",
            ApplicationRoles.Staff));

        Assert.True(principal.Identity!.IsAuthenticated);
        Assert.Equal("17", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("Staff Member", principal.Identity.Name);
        Assert.Equal("staff@example.test", principal.FindFirstValue(ClaimTypes.Email));
        Assert.True(principal.IsInRole(ApplicationRoles.Staff));
    }

    [Fact]
    public void Account_controller_depends_only_on_auth_service()
    {
        var constructor = Assert.Single(typeof(AccountController).GetConstructors());
        Assert.Equal(
            new[] { typeof(IAuthService) },
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
    }

    [Fact]
    public void Login_logout_and_dashboard_have_server_side_security_attributes()
    {
        var methods = typeof(AccountController).GetMethods(BindingFlags.Instance | BindingFlags.Public);
        var loginMethods = methods.Where(method => method.Name == nameof(AccountController.Login)).ToArray();
        Assert.Equal(2, loginMethods.Length);
        Assert.All(loginMethods, method => Assert.NotNull(method.GetCustomAttribute<AllowAnonymousAttribute>()));

        var loginPost = Assert.Single(loginMethods, method => method.GetCustomAttribute<HttpPostAttribute>() is not null);
        Assert.NotNull(loginPost.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());

        var dashboard = Assert.Single(methods, method => method.Name == nameof(AccountController.Dashboard));
        Assert.NotNull(dashboard.GetCustomAttribute<AuthorizeAttribute>());

        var logout = Assert.Single(methods, method => method.Name == nameof(AccountController.Logout));
        Assert.NotNull(logout.GetCustomAttribute<AuthorizeAttribute>());
        Assert.NotNull(logout.GetCustomAttribute<HttpPostAttribute>());
        Assert.NotNull(logout.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
    }

    [Fact]
    public async Task Cookie_validation_rejects_an_inactive_database_account()
    {
        var authService = new StubAuthService { IsActive = false };
        var authenticationService = new RecordingAuthenticationService();
        var services = new ServiceCollection()
            .AddSingleton<IAuthenticationService>(authenticationService)
            .BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = services };
        var principal = AuthenticationClaimsFactory.CreatePrincipal(new AuthenticatedUser(
            "17",
            "Staff Member",
            "staff@example.test",
            ApplicationRoles.Staff));
        var scheme = new AuthenticationScheme(
            CookieAuthenticationDefaults.AuthenticationScheme,
            null,
            typeof(CookieAuthenticationHandler));
        var ticket = new AuthenticationTicket(principal, scheme.Name);
        var context = new CookieValidatePrincipalContext(
            httpContext,
            scheme,
            new CookieAuthenticationOptions(),
            ticket);

        await new ActiveAccountCookieEvents(authService).ValidatePrincipal(context);

        Assert.Null(context.Principal);
        Assert.True(authenticationService.SignedOut);
        Assert.Equal(17, authService.LastAccountId);
    }

    [Fact]
    public async Task Cookie_validation_keeps_configuration_admin_without_database_lookup()
    {
        var authService = new StubAuthService { IsActive = false };
        var httpContext = new DefaultHttpContext();
        var principal = AuthenticationClaimsFactory.CreatePrincipal(new AuthenticatedUser(
            ApplicationRoles.AdminSubjectId,
            "Administrator",
            "admin@example.test",
            ApplicationRoles.Admin));
        var scheme = new AuthenticationScheme(
            CookieAuthenticationDefaults.AuthenticationScheme,
            null,
            typeof(CookieAuthenticationHandler));
        var context = new CookieValidatePrincipalContext(
            httpContext,
            scheme,
            new CookieAuthenticationOptions(),
            new AuthenticationTicket(principal, scheme.Name));

        await new ActiveAccountCookieEvents(authService).ValidatePrincipal(context);

        Assert.Same(principal, context.Principal);
        Assert.Null(authService.LastAccountId);
    }

    private sealed class StubAuthService : IAuthService
    {
        public bool IsActive { get; init; }
        public int? LastAccountId { get; private set; }

        public Task<AuthenticationResult> AuthenticateAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(AuthenticationResult.Failure);

        public Task<bool> IsAccountActiveAsync(
            int accountId,
            CancellationToken cancellationToken = default)
        {
            LastAccountId = accountId;
            return Task.FromResult(IsActive);
        }
    }

    private sealed class RecordingAuthenticationService : IAuthenticationService
    {
        public bool SignedOut { get; private set; }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) =>
            Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(
            HttpContext context,
            string? scheme,
            AuthenticationProperties? properties) => Task.CompletedTask;

        public Task ForbidAsync(
            HttpContext context,
            string? scheme,
            AuthenticationProperties? properties) => Task.CompletedTask;

        public Task SignInAsync(
            HttpContext context,
            string? scheme,
            ClaimsPrincipal principal,
            AuthenticationProperties? properties) => Task.CompletedTask;

        public Task SignOutAsync(
            HttpContext context,
            string? scheme,
            AuthenticationProperties? properties)
        {
            SignedOut = true;
            return Task.CompletedTask;
        }
    }
}
