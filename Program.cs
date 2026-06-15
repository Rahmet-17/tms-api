using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using TmsCore.Interfaces;
using TmsCore.Services;

var builder = WebApplication.CreateBuilder(args);

// SERVICES 
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();
builder.Services.AddSingleton<IStudentService, StudentService>();
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

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

//  APP 
var app = builder.Build();

// IMPORTANT: ProblemDetails middleware FIRST
app.UseExceptionHandler(); // ensures safe RFC 9457 responses in Production

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseStatusCodePages();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// ENDPOINTS 

// Simple test endpoints
app.MapGet("/weatherforecast", () => Results.Ok(new
{
    date = DateTime.Now,
    temperatureC = 20,
    summary = "Sunny"
}));

app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
}));


// Error test endpoint (used in checkpoint)
app.MapGet("/api/error", () =>
{
    throw new Exception(
    "Simulated database failure for ProblemDetails testing");
});

app.MapControllers();

app.Run();