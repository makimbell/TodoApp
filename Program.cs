using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MudBlazor.Services;
using TodoApp.Components;
using TodoApp.Data;
using TodoApp.Services;

namespace TodoApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // UI & Component Services
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddMudServices();

            // Data Access (IDbContextFactory + Azure SQL Transient Retry)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "todo");
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                }));

            // Application Services
            builder.Services.AddSingleton<TodoNotificationService>();

            // Authentication & Authorization
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "Microsoft";
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "TodoApp.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            })
            .AddOpenIdConnect("Microsoft", options =>
            {
                options.Authority = "https://login.microsoftonline.com/common/v2.0";
                options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]
                    ?? throw new InvalidOperationException("Microsoft ClientId is missing from configuration.");
                options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]
                    ?? throw new InvalidOperationException("Microsoft ClientSecret is missing from configuration.");
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.CallbackPath = "/signin-oidc";
                options.SaveTokens = false;

                options.Scope.Add("profile");
                options.Scope.Add("email");

                // Map Microsoft's v2.0 'name' claim and handle multi-tenant /common/v2.0 issuer validation
                options.TokenValidationParameters.NameClaimType = "name";
                options.TokenValidationParameters.IssuerValidator = (issuer, securityToken, validationParameters) =>
                {
                    // Verify the token came from login.microsoftonline.com and matches its own tenant ID
                    if (issuer.StartsWith("https://login.microsoftonline.com/", StringComparison.Ordinal)
                        && issuer.EndsWith("/v2.0", StringComparison.Ordinal))
                    {
                        return issuer;
                    }

                    throw new Microsoft.IdentityModel.Tokens.SecurityTokenInvalidIssuerException($"Invalid issuer: {issuer}");
                };

                // Prefix the provider name onto NameIdentifier so IDs never collide across providers
                options.Events.OnTicketReceived = context =>
                {
                    if (context.Principal?.Identity is ClaimsIdentity identity)
                    {
                        var sub = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (!string.IsNullOrEmpty(sub) && !sub.StartsWith("microsoft:", StringComparison.Ordinal))
                        {
                            var existing = identity.FindFirst(ClaimTypes.NameIdentifier);
                            if (existing is not null) identity.RemoveClaim(existing);
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, $"microsoft:{sub}"));
                        }
                    }
                    return Task.CompletedTask;
                };
            })
            .AddGoogle("Google", options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
                    ?? throw new InvalidOperationException("Google ClientId is missing from configuration.");
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
                    ?? throw new InvalidOperationException("Google ClientSecret is missing from configuration.");
                options.CallbackPath = "/signin-google";

                // Prefix the provider name onto NameIdentifier so IDs never collide across providers
                options.Events.OnTicketReceived = context =>
                {
                    if (context.Principal?.Identity is ClaimsIdentity identity)
                    {
                        var sub = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (!string.IsNullOrEmpty(sub) && !sub.StartsWith("google:", StringComparison.Ordinal))
                        {
                            var existing = identity.FindFirst(ClaimTypes.NameIdentifier);
                            if (existing is not null) identity.RemoveClaim(existing);
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, $"google:{sub}"));
                        }
                    }
                    return Task.CompletedTask;
                };
            });

            builder.Services.AddAuthorization();
            builder.Services.AddCascadingAuthenticationState();

            // Reverse Proxy Configuration (Required for Azure App Service / Linux containers)
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

            var app = builder.Build();

            // Middleware Pipeline (Order is critical here)
            app.UseForwardedHeaders();

            if (!app.Environment.IsDevelopment())
            {
                // createScopeForErrors: true ensures scoped services work on the /Error page
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.MapStaticAssets();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            // Auth Endpoints (with Local URL validation to prevent Open Redirects)
            app.MapGet("/auth/login/microsoft", (string? returnUrl) =>
            {
                var safeReturnUrl = (!string.IsNullOrWhiteSpace(returnUrl)
                                     && returnUrl.StartsWith('/')
                                     && !returnUrl.StartsWith("//")
                                     && !returnUrl.StartsWith("/\\"))
                    ? returnUrl
                    : "/";

                return Results.Challenge(
                    new AuthenticationProperties { RedirectUri = safeReturnUrl },
                    ["Microsoft"]);
            });

            app.MapGet("/auth/login/google", (string? returnUrl) =>
            {
                var safeReturnUrl = (!string.IsNullOrWhiteSpace(returnUrl)
                                     && returnUrl.StartsWith('/')
                                     && !returnUrl.StartsWith("//")
                                     && !returnUrl.StartsWith("/\\"))
                    ? returnUrl
                    : "/";

                return Results.Challenge(
                    new AuthenticationProperties { RedirectUri = safeReturnUrl },
                    ["Google"]);
            });

            app.MapGet("/auth/logout", async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Redirect("/");
            });

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}