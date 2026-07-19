using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TmsApi.Infrastructure.Persistence.Data;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Services;
namespace TmsApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TmsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IAssessmentService, AssessmentService>();
        services.AddScoped<IStudentService, StudentService>();
        return services;
    }
}