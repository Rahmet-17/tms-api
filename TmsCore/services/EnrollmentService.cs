using Microsoft.Extensions.Logging;
using TmsCore.Interfaces;
using TmsCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TmsCore.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly Dictionary<string, EnrollmentRecord> _store = new();
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(ILogger<EnrollmentService> logger)
    {
        _logger = logger;
    }

    public Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
    {
        var existing = _store.Values.FirstOrDefault(e =>
            e.StudentId == studentId && e.CourseCode == courseCode);

        if (existing != null)
        {
            _logger.LogWarning("Duplicate enrollment for {StudentId}", studentId);
            return Task.FromResult(existing);
        }

        var record = new EnrollmentRecord(
            Guid.NewGuid().ToString("N")[..8],
            studentId,
            courseCode,
            DateTime.UtcNow
        );

        _store[record.Id] = record;

        _logger.LogInformation("Created enrollment {Id}", record.Id);

        return Task.FromResult(record);
    }

    public Task<EnrollmentRecord?> GetByIdAsync(string id)
        => Task.FromResult(_store.TryGetValue(id, out var r) ? r : null);

    public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
        => Task.FromResult((IReadOnlyList<EnrollmentRecord>)_store.Values.ToList());

    public Task<bool> DeleteAsync(string id)
        => Task.FromResult(_store.Remove(id));
}