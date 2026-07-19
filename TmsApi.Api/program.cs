using Microsoft.AspNetCore.Authentication.JwtBearer;
using TmsApi;
using TmsApi.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using TmsApi.Infrastructure.Services;
using TmsApi.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence.Data;
using TmsApi.Application.Filters;
using Asp.Versioning;
using TmsApi.Application.Middleware;
using TmsApi.Infrastructure;


var builder = WebApplication.CreateBuilder(args);


// SERVICES 
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});


// OpenAPI Documents (v1 + v2)
builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v1";
});

builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v2";
});


// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);

    options.AssumeDefaultVersionWhenUnspecified = true;

    options.ReportApiVersions = true;

    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";

    options.SubstituteApiVersionInUrl = true;
});


builder.Services.AddProblemDetails();


// DbContext
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        


// Application Services
builder.Services.AddInfrastructure(builder.Configuration);


// Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false
        };
    });


builder.Services.AddAuthorization();


// Options
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();


// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();


var app = builder.Build();


// PIPELINE

app.UseDeveloperExceptionPage();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{documentName}.json");

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("TMS API Reference")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(
                ScalarTarget.CSharp,
                ScalarClient.HttpClient);

        options
            .AddDocument("v1", "API Version 1.0")
            .AddDocument("v2", "API Version 2.0");
    });
}


app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();
app.UseMiddleware<V1DeprecationMiddleware>();
app.MapControllers();
app.Run();