using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using TmsApi.Application.Filters;
using TmsApi.Application.Middleware;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Infrastructure;
using TmsApi.Infrastructure.Persistence.Data;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Api.RateLimiting;
using TmsApi.Infrastructure.Transcripts;
using System.Threading.Channels;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Workers;
using TmsApi.Api.Hubs;
using TmsApi.Api.Notifications;
using TmsApi.Application.Notifications;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using TmsApi.Domain.Entities;
using System.Text;
using TmsApi.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using TmsApi.Api.Authorization;

var builder = WebApplication.CreateBuilder(args);

// USER SECRETS

builder.Configuration.AddUserSecrets("TmsApi-Development");

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// TRANSCRIPT / SIGNALR

builder.Services.AddSingleton<
    ITranscriptNotificationService,
    SignalRTranscriptNotificationService>();

builder.Services.AddSignalR();

builder.Services.AddHostedService<TranscriptWorker>();

builder.Services.AddSingleton<Channel<TranscriptRequest>>(
    Channel.CreateBounded<TranscriptRequest>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        }));

builder.Services.AddSingleton<
    ITranscriptStatusStore,
    InMemoryTranscriptStatusStore>();

// ANTIFORGERY

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});

// CONTROLLERS

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});

// OPENAPI

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

// API VERSIONING

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);

    options.AssumeDefaultVersionWhenUnspecified = true;

    options.ReportApiVersions = true;

    options.ApiVersionReader =
        new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";

    options.SubstituteApiVersionInUrl = true;
});

// PROBLEM DETAILS

builder.Services.AddProblemDetails();

// DATABASE

builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase")));

// INFRASTRUCTURE

builder.Services.AddInfrastructure(
    builder.Configuration);

// ASP.NET IDENTITY

builder.Services.AddIdentityCore<TmsUser>(
    options =>
    {
        // Password policy

        options.Password.RequiredLength = 12;

        options.Password.RequireUppercase = true;

        options.Password.RequireDigit = true;

        options.Password.RequireNonAlphanumeric = true;

        // Lockout policy

        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TmsDbContext>();

// JWT CONFIGURATION

builder.Services.AddScoped<TokenService>();

var jwtKey = builder.Configuration["Jwt:Key"];

var jwtIssuer = builder.Configuration["Jwt:Issuer"];

var jwtAudience = builder.Configuration["Jwt:Audience"];

var jwtExpiryMinutes =
    builder.Configuration["Jwt:ExpiryMinutes"];

// Validate JWT configuration

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT Key is missing. Make sure Jwt:Key is configured in User Secrets.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException(
        "JWT Issuer is missing. Check Jwt:Issuer in appsettings.Development.json.");
}

if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "JWT Audience is missing. Check Jwt:Audience in appsettings.Development.json.");
}

if (string.IsNullOrWhiteSpace(jwtExpiryMinutes))
{
    throw new InvalidOperationException(
        "JWT ExpiryMinutes is missing. Check Jwt:ExpiryMinutes in appsettings.Development.json.");
}

// JWT BEARER AUTHENTICATION

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,

            ValidateAudience = true,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,

            ValidAudience = jwtAudience,

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey))
        };
});

// AUTHORIZATION

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanEditCourse", policy =>
    {
        policy.Requirements.Add(
            new CourseInstructorRequirement());
    });

builder.Services.AddSingleton<
    IAuthorizationHandler,
    CourseInstructorHandler>();

// CORS

var allowedOrigins =
    builder.Configuration
        .GetSection("AllowedOrigins")
        .Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("TmsClient", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(
                TimeSpan.FromMinutes(10));
    });
});

// PAYMENT OPTIONS

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// LOGGING

builder.Logging.ClearProviders();

builder.Logging.AddConsole();

// MEDIATR

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(EnrollStudentHandler).Assembly));

// FLUENT VALIDATION

builder.Services.AddValidatorsFromAssembly(
    typeof(EnrollStudentValidator).Assembly);

// PIPELINE BEHAVIORS

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

// RATE LIMITING

builder.Services.AddRateLimiter(options =>
{
    // AUTHENTICATION RATE LIMITER
    // Maximum 5 login attempts per minute

    options.AddFixedWindowLimiter(
        "AuthLimiter",
        opt =>
        {
            opt.PermitLimit = 5;

            opt.Window =
                TimeSpan.FromMinutes(1);

            opt.QueueLimit = 0;
        });

    // GLOBAL TOKEN BUCKET LIMITER

    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext =>
            {
                var (partitionKey, tier) =
                    ApiKeyResolver.Resolve(httpContext);

                return tier switch
                {
                    ApiKeyTier.Paid =>
                        RateLimitPartition
                            .GetTokenBucketLimiter(
                                $"paid:{partitionKey}",
                                _ =>
                                    new TokenBucketRateLimiterOptions
                                    {
                                        TokenLimit = 200,

                                        TokensPerPeriod = 100,

                                        ReplenishmentPeriod =
                                            TimeSpan.FromSeconds(10),

                                        QueueLimit = 0,

                                        AutoReplenishment = true
                                    }),

                    ApiKeyTier.Free =>
                        RateLimitPartition
                            .GetTokenBucketLimiter(
                                $"free:{partitionKey}",
                                _ =>
                                    new TokenBucketRateLimiterOptions
                                    {
                                        TokenLimit = 30,

                                        TokensPerPeriod = 10,

                                        ReplenishmentPeriod =
                                            TimeSpan.FromSeconds(10),

                                        QueueLimit = 0,

                                        AutoReplenishment = true
                                    }),

                    _ =>
                        RateLimitPartition
                            .GetTokenBucketLimiter(
                                $"anon:{partitionKey}",
                                _ =>
                                    new TokenBucketRateLimiterOptions
                                    {
                                        TokenLimit = 10,

                                        TokensPerPeriod = 5,

                                        ReplenishmentPeriod =
                                            TimeSpan.FromSeconds(10),

                                        QueueLimit = 0,

                                        AutoReplenishment = true
                                    })
                };
            });

    // TRANSCRIPT CONCURRENCY LIMITER

    options.AddConcurrencyLimiter(
        "transcripts",
        limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;

            limiterOptions.QueueLimit = 20;

            limiterOptions.QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst;
        });

    // SEARCH TOKEN BUCKET

    options.AddTokenBucketLimiter(
        "search",
        opt =>
        {
            opt.TokenLimit = 10;

            opt.TokensPerPeriod = 5;

            opt.ReplenishmentPeriod =
                TimeSpan.FromSeconds(10);

            opt.QueueLimit = 2;
        });

    // RATE LIMIT RESPONSE

    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, ct) =>
    {
        Console.WriteLine(
            $"RATE LIMIT BLOCKED: {context.HttpContext.Request.Path}");

        var retryAfter = "10";

        if (context.Lease.TryGetMetadata(
            MetadataName.RetryAfter,
            out var retry))
        {
            retryAfter =
                ((int)retry.TotalSeconds).ToString();
        }

        context.HttpContext.Response.Headers.RetryAfter =
            retryAfter;

        await context.HttpContext.Response
            .WriteAsJsonAsync(
                new
                {
                    title = "Rate limit exceeded",

                    detail =
                        $"Too many requests. Retry after {retryAfter} seconds",

                    status = 429,

                    type =
                        "https://tms.local/errors/rate_limit_exceeded"
                },
                ct);
    };
});

// HYBRID CACHE

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions =
        new HybridCacheEntryOptions
        {
            Expiration =
                TimeSpan.FromMinutes(10),

            LocalCacheExpiration =
                TimeSpan.FromMinutes(2)
        };
});

// EXCEPTION HANDLING

builder.Services.AddExceptionHandler<
    GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

// BUILD APPLICATION

var app = builder.Build();

// EXCEPTION HANDLER

app.UseExceptionHandler();

// SECURITY RESPONSE HEADERS

app.Use(async (context, next) =>
{
    context.Response.Headers.Append(
        "X-Content-Type-Options",
        "nosniff");

    context.Response.Headers.Append(
        "X-Frame-Options",
        "DENY");

    context.Response.Headers.Append(
        "Referrer-Policy",
        "strict-origin-when-cross-origin");

    // context.Response.Headers.Append(
    //     "Content-Security-Policy",
    //     "default-src 'self'; " +
    //     "script-src 'self'; " +
    //     "style-src 'self' 'unsafe-inline';");

    await next();
});

// SIGNALR HUB

app.MapHub<TmsHub>("/hubs/tms")
    .RequireCors("TmsClient");

// SCALAR / OPENAPI

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(
        "/openapi/{documentName}.json");

    app.MapScalarApiReference(
        options =>
        {
            options
                .WithTitle("TMS API Reference")
                .WithTheme(
                    ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(
                    ScalarTarget.CSharp,
                    ScalarClient.HttpClient);

            options
                .AddDocument(
                    "v1",
                    "API Version 1.0")
                .AddDocument(
                    "v2",
                    "API Version 2.0");
        });
}

// XSRF TOKEN

app.Use(async (context, next) =>
{
    if (
        context.User.Identity?.IsAuthenticated == true
        ||
        context.Request.Cookies.ContainsKey("tms_auth"))
    {
        var antiforgery =
            context.RequestServices
                .GetRequiredService<IAntiforgery>();

        var tokens =
            antiforgery.GetAndStoreTokens(context);

        context.Response.Cookies.Append(
            "XSRF-TOKEN",
            tokens.RequestToken!,
            new CookieOptions
            {
                HttpOnly = false,

                Secure =
                    !builder.Environment.IsDevelopment(),

                SameSite =
                    SameSiteMode.Strict
            });
    }

    await next(context);
});

// MIDDLEWARE PIPELINE

app.UseCors("TmsClient");

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

// API VERSION DEPRECATION

app.UseMiddleware<V1DeprecationMiddleware>();

// CONTROLLERS

app.MapControllers();

// RUN

app.Run();