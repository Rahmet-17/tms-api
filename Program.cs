
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    
       options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false 
    });
 builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseExceptionHandler("/error");

app.UseMiddleware<RequestLoggingMiddleware>();


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//app.Map("/error", () =>
{
    //return Results.Problem("An unexpected error occurred.");
//});
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
}))

.RequireAuthorization();

;app.Run();}