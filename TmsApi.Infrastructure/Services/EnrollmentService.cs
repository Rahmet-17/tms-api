using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence.Data;
using TmsApi.Domain.Entities;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;



namespace TmsApi.Infrastructure.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly TmsDbContext context;

    public EnrollmentService(TmsDbContext context)
    {
        this.context = context;
    }

   
    // GET BY ID
   
    public async Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt))
            .FirstOrDefaultAsync(ct);
    }

    
    // CREATE ENROLLMENT
  
    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        //  Get course with enrollments
        var course = await context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == courseId, ct);

        //  Course not found
        if (course is null)
            throw new InvalidOperationException("Course not found");

        // Check capacity
        if (course.Enrollments.Count >= course.MaxCapacity)
            throw new InvalidOperationException("Course is full");

        //  Create enrollment
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };

        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);

        //  Return DTO
        return new EnrollmentResponseDto(
            enrollment.Id,
            enrollment.CourseId,
            enrollment.StudentId,
            enrollment.EnrolledAt);
    }


    // GET ALL (optional but needed if interface has it)
    
    public async Task<List<EnrollmentResponseDto>> GetAllAsync()
    {
        return await context.Enrollments
            .AsNoTracking()
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt))
            .ToListAsync();
    }

   
    // DELETE
   
    public async Task<bool> DeleteAsync(int id)
    {
        var enrollment = await context.Enrollments.FindAsync(id);

        if (enrollment is null)
            return false;

        context.Enrollments.Remove(enrollment);
        await context.SaveChangesAsync();

        return true;
    }


    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(
    int courseId,
    CancellationToken ct)
{
    return await context.Enrollments
        .AsNoTracking()
        .Where(e => e.CourseId == courseId)
        .Select(e => new EnrollmentResponseDto(
            e.Id,
            e.CourseId,
            e.StudentId,
            e.EnrolledAt
        ))
        .ToListAsync(ct);
}
}