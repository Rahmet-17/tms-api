using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence.Data;

namespace TmsApi.Infrastructure.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly TmsDbContext context;

    public EnrollmentService(TmsDbContext context)
    {
        this.context = context;
    }


    // GET ENROLLMENT BY ID
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


    // CREATE ENROLLMENT (M6)
    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        var course = await context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == courseId, ct);


        if (course is null)
            throw new InvalidOperationException("Course not found");


        if (course.Enrollments.Count >= course.MaxCapacity)
            throw new InvalidOperationException("Course is full");


        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };


        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);


        return new EnrollmentResponseDto(
            enrollment.Id,
            enrollment.CourseId,
            enrollment.StudentId,
            enrollment.EnrolledAt);
    }



    // GET ALL
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
        var enrollment = await context.Enrollments
            .FindAsync(id);


        if (enrollment is null)
            return false;


        context.Enrollments.Remove(enrollment);

        await context.SaveChangesAsync();


        return true;
    }



    // CHECK DUPLICATE ENROLLMENT (M7 CQRS)
    public async Task<bool> ExistsAsync(
        int studentId,
        string courseCode,
        CancellationToken ct)
    {
        return await context.Enrollments
            .AnyAsync(
                e => e.StudentId == studentId &&
                     e.Course.Code == courseCode,
                ct);
    }



    // ADD ENROLLMENT (M7 CQRS)
    public async Task AddAsync(
        Enrollment enrollment,
        CancellationToken ct)
    {
        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);
    }



    // GET STUDENT SCHEDULE (M7 CQRS)
    public async Task<List<Enrollment>> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct)
    {
        return await context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .ToListAsync(ct);
    }



    // GET ENROLLMENTS BY COURSE
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
                e.EnrolledAt))
            .ToListAsync(ct);
    }
}