using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

public static class WebAppExtension
{
    public const string AaasPolicy = nameof(AaasPolicy);
    public static void AddAaasForApi(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
        options =>
        {
            options.Authority = builder.Configuration["Aaas:Authority"];
            options.Audience = builder.Configuration["Aaas:Audience"];
            options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    Microsoft.Extensions.Primitives.StringValues accessToken = context.Request.Query["access_token"];
                    Microsoft.AspNetCore.Http.PathString path = context.HttpContext.Request.Path;
                    if(!string.IsNullOrEmpty(accessToken) &&
                        (path.StartsWithSegments("/hubs")))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });
        builder.Services.AddAuthorization(options => options.AddPolicy(AaasPolicy, policy => policy.RequireClaim("scope", builder.Configuration["Aaas:Scope"])));
        builder.Services.AddControllers(config => config.Filters.Add(new AuthorizeFilter(AaasPolicy)));
        builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
    }
}