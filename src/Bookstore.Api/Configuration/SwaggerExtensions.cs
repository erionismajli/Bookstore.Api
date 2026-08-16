using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bookstore.Api.Configuration;

public static class SwaggerExtensions
{
    public const string ClientCredentialsScheme = "clientCredentials";
    public const string ImplicitScheme = "implicit";

    public static IServiceCollection AddBookstoreSwagger(this IServiceCollection services, string identityServerUrl)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Bookstore API",
                Version = "v1",
                Description = "CRUD operations for books and authors, with paginated book search."
            });

            options.AddSecurityDefinition(ClientCredentialsScheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    ClientCredentials = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri($"{identityServerUrl}/connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            [AuthExtensions.BookCrudScope] = "Create, read, update and delete books"
                        }
                    }
                }
            });

            options.AddSecurityDefinition(ImplicitScheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{identityServerUrl}/connect/authorize"),
                        Scopes = new Dictionary<string, string>
                        {
                            [AuthExtensions.BookSearchScope] = "Search books by title and author"
                        }
                    }
                }
            });

            options.OperationFilter<AuthorizeOperationFilter>();
        });

        return services;
    }
}

internal sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var attributes = context.MethodInfo.GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .Concat(context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>() ?? []);
        var policy = attributes.Select(attribute => attribute.Policy).FirstOrDefault(value => value is not null);

        var requirement = policy switch
        {
            AuthExtensions.BookCrudPolicy =>
                (SwaggerExtensions.ClientCredentialsScheme, AuthExtensions.BookCrudScope),
            AuthExtensions.BookSearchPolicy =>
                (SwaggerExtensions.ImplicitScheme, AuthExtensions.BookSearchScope),
            _ => ((string Scheme, string Scope)?)null
        };

        if (requirement is null)
        {
            return;
        }

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = requirement.Value.Scheme
                    }
                }] = [requirement.Value.Scope]
            }
        ];
    }
}
