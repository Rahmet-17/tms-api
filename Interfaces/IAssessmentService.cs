using TmsApi.Entities;

namespace TmsApi.Interfaces;

public interface IAssessmentService
{
    Task<Assessment> CreateAsync(
        string title,
        decimal maxScore,
        decimal weight,
        int courseId);

    Task<Assessment?> GetByIdAsync(int id);

    Task<IReadOnlyList<Assessment>> GetAllAsync();

    Task<bool> DeleteAsync(int id);
}