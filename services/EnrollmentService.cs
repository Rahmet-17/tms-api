using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Interfaces;

namespace TmsApi.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly TmsDbContext _context;
        private readonly ILogger<EnrollmentService> _logger;

        public EnrollmentService(
            TmsDbContext context,
            ILogger<EnrollmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Enrollment> EnrollAsync(
            int studentId,
            int courseId)
        {
            var existing = await _context.Enrollments
                .FirstOrDefaultAsync(e =>
                    e.StudentId == studentId &&
                    e.CourseId == courseId);

            if (existing != null)
            {
                _logger.LogWarning(
                    "Student {StudentId} already enrolled in Course {CourseId}",
                    studentId,
                    courseId);

                return existing;
            }

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Student {StudentId} enrolled in Course {CourseId}",
                studentId,
                courseId);

            return enrollment;
        }

        public async Task<Enrollment?> GetByIdAsync(int id)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IReadOnlyList<Enrollment>> GetAllAsync()
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrollment == null)
            {
                _logger.LogWarning(
                    "Enrollment {EnrollmentId} not found",
                    id);

                return false;
            }

            _context.Enrollments.Remove(enrollment);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Deleted Enrollment {EnrollmentId}",
                id);

            return true;
        }
    }
}