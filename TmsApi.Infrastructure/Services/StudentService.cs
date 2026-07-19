using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence.Data;
using TmsApi.Application.Dtos;
using TmsApi.Domain.Entities;
using TmsApi.Application.Interfaces;

namespace TmsApi.Infrastructure.Services;
public class StudentService : IStudentService
{
    private readonly TmsDbContext context;

    public StudentService(TmsDbContext context)
    {
        this.context = context;
    }

    public async Task<IReadOnlyList<StudentResponseDto>> GetAllAsync(
        CancellationToken ct)
    {
        return await context.Students
            .AsNoTracking()
            .Select(s => new StudentResponseDto(
                s.Id,
                s.Name,
                s.RegistrationNumber))
            .ToListAsync(ct);
    }

    public async Task<StudentResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        return await context.Students
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentResponseDto(
                s.Id,
                s.Name,
                s.RegistrationNumber))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<StudentResponseDto> CreateAsync(
        CreateStudentRequest request,
        CancellationToken ct)
    {
        var student = new Student
        {
            Name = request.Name,
            RegistrationNumber = request.RegistrationNumber
        };

        context.Students.Add(student);

        await context.SaveChangesAsync(ct);

        return new StudentResponseDto(
            student.Id,
            student.Name,
            student.RegistrationNumber);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken ct)
    {
        var student = await context.Students
            .FindAsync([id], ct);

        if (student is null)
            return false;

        context.Students.Remove(student);

        await context.SaveChangesAsync(ct);

        return true;
    }
}