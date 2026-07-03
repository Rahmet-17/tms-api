using TmsApi.Entities;

namespace TmsApi.Interfaces;

public interface IStudentService
{
    Task<Student> CreateAsync(string name, decimal? gpa);
    Task<Student?> GetByIdAsync(int id);
    Task<IReadOnlyList<Student>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
}