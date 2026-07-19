using TmsApi.Application.Dtos;

namespace TmsApi.Application.Interfaces;

public interface IStudentService
{
    Task<IReadOnlyList<StudentResponseDto>> GetAllAsync(
        CancellationToken ct);

    Task<StudentResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct);

    Task<StudentResponseDto> CreateAsync(
        CreateStudentRequest request,
        CancellationToken ct);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken ct);
}