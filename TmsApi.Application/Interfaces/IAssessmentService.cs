using TmsApi.Application.Dtos;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

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