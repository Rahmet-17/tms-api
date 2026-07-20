using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using FluentValidation;
using MediatR;

using TmsApi.Application.Filters;
using TmsApi.Application.Middleware;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Infrastructure;
using TmsApi.Infrastructure.Persistence.Data;


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


// Database
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// Infrastructure
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


// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(EnrollStudentHandler).Assembly));


// FluentValidation
builder.Services.AddValidatorsFromAssembly(
    typeof(EnrollStudentValidator).Assembly);


// Pipeline Behaviors
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));


// Exception Handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();


var app = builder.Build();


// Exception Handler
app.UseExceptionHandler();


// Development
//app.UseDeveloperExceptionPage();


if (app.Environment.IsDevelopment())
{
    // Generates:
    // /openapi/v1.json
    // /openapi/v2.json
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


// v1 deprecation middleware
app.UseMiddleware<V1DeprecationMiddleware>();


app.MapControllers();


app.Run();