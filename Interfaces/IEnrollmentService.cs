using TmsApi.Entities;

namespace TmsApi.Interfaces
{
    public interface IEnrollmentService
    {
        Task<Enrollment> EnrollAsync(int studentId, int courseId);

        Task<Enrollment?> GetByIdAsync(int id);

        Task<IReadOnlyList<Enrollment>> GetAllAsync();

        Task<bool> DeleteAsync(int id);
    }
}