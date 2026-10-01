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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StudentNameMVC.ViewModels;

namespace AIVES.Tests;

public sealed class AuthenticationPresentationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("valid-token")]
    public async Task Recovery_request_keeps_same_response_for_unknown_email_and_delivery_failure(string? token)
    {
        var controller = new AccountController(new StubAuthService { ResetToken = token },
            new FailingEmailSender(), NullLogger<AccountController>.Instance);
        var response = Assert.IsType<ViewResult>(await controller.ForgotPassword(
            new ForgotPasswordViewModel { Email = "member@example.test" }, default));
        var model = Assert.IsType<ForgotPasswordViewModel>(response.Model);
        Assert.True(model.Submitted);
        Assert.Empty(model.Email);
        Assert.True(controller.ModelState.IsValid);
    }

    [Theory]
    [InlineData(nameof(AccountController.ForgotPassword))]
    [InlineData(nameof(AccountController.ResetPassword))]
    public void Recovery_posts_allow_anonymous_and_require_antiforgery(string action)
    {
        var method = Assert.Single(typeof(AccountController).GetMethods(),
            method => method.Name == action && method.GetCustomAttribute<HttpPostAttribute>() is not null);
        Assert.NotNull(method.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.NotNull(method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
    }

    private sealed class FailingEmailSender : IPasswordResetEmailSender
    {
        public Task SendAsync(string email, string token, CancellationToken ct) =>
            throw new InvalidOperationException("Test delivery failure");
    }

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
            new[] { typeof(IAuthService), typeof(IPasswordResetEmailSender), typeof(ILogger<AccountController>) },
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
        var authService = new StubAuthService { IsSessionValid = false };
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
        Assert.Equal(ApplicationRoles.Staff, authService.LastClaimedRole);
    }

    [Fact]
    public async Task Cookie_validation_rejects_a_stale_database_role()
    {
        var authService = new StubAuthService { IsSessionValid = false };
        var authenticationService = new RecordingAuthenticationService();
        var services = new ServiceCollection()
            .AddSingleton<IAuthenticationService>(authenticationService)
            .BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = services };
        var principal = AuthenticationClaimsFactory.CreatePrincipal(new AuthenticatedUser(
            "17",
            "Former Staff Member",
            "member@example.test",
            ApplicationRoles.Staff));
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

        Assert.Null(context.Principal);
        Assert.True(authenticationService.SignedOut);
        Assert.Equal(ApplicationRoles.Staff, authService.LastClaimedRole);
    }

    [Fact]
    public async Task Cookie_validation_keeps_configuration_admin_without_database_lookup()
    {
        var authService = new StubAuthService { IsSessionValid = false };
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
        public string? ResetToken { get; init; }
        public Task<string?> CreatePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(ResetToken);
        public Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public bool IsSessionValid { get; init; }
        public int? LastAccountId { get; private set; }
        public string? LastClaimedRole { get; private set; }

        public Task<AuthenticationResult> AuthenticateAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(AuthenticationResult.Failure);

        public Task<bool> IsAccountSessionValidAsync(
            int accountId,
            string claimedRole,
            CancellationToken cancellationToken = default)
        {
            LastAccountId = accountId;
            LastClaimedRole = claimedRole;
            return Task.FromResult(IsSessionValid);
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
