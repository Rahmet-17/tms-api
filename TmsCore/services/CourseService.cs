using Microsoft.Extensions.Logging;
using TmsCore.Interfaces;
using TmsCore.Models;

namespace TmsCore.Services;

public class CourseService : ICourseService
{
    private readonly Dictionary<string, Course> _courses = new();
    private readonly ILogger<CourseService> _logger;

    public CourseService(ILogger<CourseService> logger)
    {
        _logger = logger;
    }

    public Task<Course> CreateAsync(string code, string title, int capacity)
    {
        if (_courses.ContainsKey(code))
        {
            _logger.LogWarning("Course already exists: {Code}", code);
            return Task.FromResult(_courses[code]);
        }

        var course = new Course
        {
            Code = code,
            Title = title,
            Capacity = capacity,
            EnrolledCount = 0
        };

        _courses[code] = course;

        _logger.LogInformation("Created course: {Code}", code);

        return Task.FromResult(course);
    }

    public Task<Course?> GetByIdAsync(string code)
    {
        _courses.TryGetValue(code, out var course);
        return Task.FromResult(course);
    }

    public Task<IReadOnlyList<Course>> GetAllAsync()
        => Task.FromResult((IReadOnlyList<Course>)_courses.Values.ToList());

    public Task<bool> DeleteAsync(string code)
        => Task.FromResult(_courses.Remove(code));
}