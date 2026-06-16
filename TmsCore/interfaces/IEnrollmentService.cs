using TmsCore.Models;

namespace TmsCore.Interfaces;

public interface IEnrollmentService
{
    Task<List<EnrollmentRecords>> GetAllAsync();
    Task<EnrollmentRecords?> GetByIdAsync(string id);
    Task<EnrollmentRecords> EnrollAsync(EnrollmentRecords enrollment);
    Task<bool> DeleteAsync(string id);
}