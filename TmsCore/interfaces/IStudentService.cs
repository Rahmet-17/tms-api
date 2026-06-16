using TmsCore.Models;

namespace TmsCore.Interfaces;

public interface IStudentService
{
    Task<StudentRecord> CreateAsync(string name, double? gpa);
    Task<StudentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<StudentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}