using Bookstore.Api.Data;
using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

namespace Bookstore.Api.Configuration;

public static class AuthExtensions
{
    public const string BookCrudPolicy = "BookCrud";
    public const string BookCrudScope = "bookstore.books";
    public const string BookSearchPolicy = "BookSearch";
    public const string BookSearchScope = "bookstore.search";
    public const string BookCrudClientId = "bookstore-client";
    public const string SwaggerClientId = "bookstore-swagger";

    public static IServiceCollection AddBookstoreAuth(
        this IServiceCollection services,
        string authority,
        string clientSecret)
    {
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<BookstoreDbContext>()
            .AddDefaultTokenProviders();

        services
            .AddIdentityServer(options =>
            {
                options.EmitStaticAudienceClaim = true;
                options.KeyManagement.Enabled = false;
                options.Authentication.CheckSessionCookieSameSiteMode = SameSiteMode.Lax;
            })
            .AddDeveloperSigningCredential()
            .AddInMemoryApiScopes(AuthConfig.ApiScopes)
            .AddInMemoryClients(AuthConfig.GetClients(authority, clientSecret))
            .AddAspNetIdentity<IdentityUser>();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.SlidingExpiration = true;
        });

        services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters.ValidateAudience = false;
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(BookCrudPolicy, policy =>
            {
                policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", BookCrudScope);
            });

            options.AddPolicy(BookSearchPolicy, policy =>
            {
                policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", BookSearchScope);
            });
        });

        return services;
    }
}

public static class AuthConfig
{
    public static IReadOnlyCollection<ApiScope> ApiScopes =>
    [
        new(AuthExtensions.BookCrudScope, "Manage books"),
        new(AuthExtensions.BookSearchScope, "Search books")
    ];

    public static IReadOnlyCollection<Client> GetClients(string authority, string clientSecret) =>
    [
        new()
        {
            ClientId = AuthExtensions.BookCrudClientId,
            ClientName = "Bookstore API Client",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret(clientSecret.Sha256()) },
            AllowedScopes = { AuthExtensions.BookCrudScope }
        },
        new()
        {
            ClientId = AuthExtensions.SwaggerClientId,
            ClientName = "Bookstore Swagger UI",
            AllowedGrantTypes = GrantTypes.Implicit,
            AllowAccessTokensViaBrowser = true,
            RedirectUris = { $"{authority}/swagger/oauth2-redirect.html" },
            AllowedCorsOrigins = { authority },
            AllowedScopes = { AuthExtensions.BookSearchScope }
        }
    ];
}
