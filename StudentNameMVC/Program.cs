using AIVES.BLL.Interfaces;
using AIVES.BLL.Options;
using AIVES.BLL.Security;
using AIVES.BLL.Services;
using AIVES.DAL;
using AIVES.DAL.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StudentNameMVC.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
builder.Services.AddAivesDataAccess(connectionString);

builder.Services
    .AddOptions<DefaultAdminOptions>()
    .Bind(builder.Configuration.GetSection(DefaultAdminOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Email) &&
                   !string.IsNullOrWhiteSpace(options.Password),
        "DefaultAdmin:Email and DefaultAdmin:Password are required.")
    .ValidateOnStart();

builder.Services.AddScoped<IPasswordHasher<SystemAccount>, PasswordHasher<SystemAccount>>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
builder.Services.AddScoped<ActiveAccountCookieEvents>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AIVES.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.EventsType = typeof(ActiveAccountCookieEvents);
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.RequireRole(ApplicationRoles.Admin));
    options.AddPolicy(AuthorizationPolicies.StaffOnly, policy =>
        policy.RequireRole(ApplicationRoles.Staff));
    options.AddPolicy(AuthorizationPolicies.LecturerOnly, policy =>
        policy.RequireRole(ApplicationRoles.Lecturer));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
