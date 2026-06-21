using TmsApi.Entities;

namespace TmsApi.Interfaces
{
    public interface ICourseService
    {
        Task<Course> CreateAsync(string code, string title, int capacity);

        Task<Course?> GetByIdAsync(int id);

        Task<IReadOnlyList<Course>> GetAllAsync();

        Task<bool> DeleteAsync(int id);
    }
}