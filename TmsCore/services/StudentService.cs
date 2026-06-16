using Microsoft.Extensions.Logging;
using TmsCore.Interfaces;
using TmsCore.Models;

namespace TmsCore.Services;

public class StudentService : IStudentService
{
    private readonly Dictionary<string, StudentRecord> _store = new();
    private readonly ILogger<StudentService> _logger;

    public StudentService(ILogger<StudentService> logger)
    {
        _logger = logger;
    }

    public Task<StudentRecord> CreateAsync(string name, double? gpa)
    {
        var id = Guid.NewGuid().ToString("N")[..8];

        var student = new StudentRecord(
            id,
            name,
            DateTime.UtcNow,
            gpa);

        _store[id] = student;

        _logger.LogInformation("Created student {Id}", id);

        return Task.FromResult(student);
    }

    public Task<StudentRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var student);
        return Task.FromResult(student);
    }

    public Task<IReadOnlyList<StudentRecord>> GetAllAsync()
    {
        return Task.FromResult(
            (IReadOnlyList<StudentRecord>)_store.Values.ToList());
    }

    public Task<bool> DeleteAsync(string id)
    {
        return Task.FromResult(_store.Remove(id));
    }
}