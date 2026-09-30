using LedgerPro.SubscriptionBilling.Api.Models.Responses;
using LedgerPro.SubscriptionBilling.Api.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

#region Core Services

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

#endregion

#region Custom Services

RegisterServciesHelper.RegisteredServices(builder);

#endregion

#region Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Enter the token like this: Bearer {your token}"
    });

    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

#endregion

#region JWT Authentication

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey!)
            ),

            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine(
                    "JWT Auth Failed: " +
                    context.Exception.Message
                );

                return Task.CompletedTask;
            },

            OnTokenValidated = context =>
            {
                Console.WriteLine("JWT Token Validated");

                return Task.CompletedTask;
            }
        };
    });

#endregion

#region Authorization

builder.Services.AddAuthorization();

#endregion

var app = builder.Build();

#region Global Exception Handling

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger =
            context.RequestServices
                .GetRequiredService<ILogger<Program>>();

        var exceptionFeature =
            context.Features.Get<IExceptionHandlerFeature>();

        logger.LogError(
            exceptionFeature?.Error,
            "Unhandled exception. TraceId: {TraceId}",
            context.TraceIdentifier
        );

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var response =
            ApiResponse<object>.ServerError(
                "An unexpected error occurred.",
                context.TraceIdentifier
            );

        var json =
            JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    });
});

#endregion

#region HTTP Request Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();