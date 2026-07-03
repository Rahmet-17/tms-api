using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Interfaces;

namespace TmsApi.Services;

public class StudentService : IStudentService
{
    
    private readonly TmsDbContext _context;
    private readonly ILogger<StudentService> _logger;

    

    public StudentService(TmsDbContext context, ILogger<StudentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Student> CreateAsync(string name, decimal? gpa)
    {
        var existing = await _context.Students
            .FirstOrDefaultAsync(s => s.Name == name);

        if (existing != null)
        {
            _logger.LogWarning("Duplicate student {Name}", name);
            return existing;
        }

        var student = new Student
        {
            Name = name,
            RegistrationNumber = $"STU-{Guid.NewGuid().ToString("N")[..6]}",
            GPA = gpa ?? 0m
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return student;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync()
    {
        return await _context.Students
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
            return false;

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();

        return true;
    }

    
}