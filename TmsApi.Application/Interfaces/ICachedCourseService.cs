using TmsApi.Application.Dtos;

namespace TmsApi.Application.Interfaces;

public interface ICachedCourseService
{
    Task<CourseResponseDto> GetCourseAsync(
        string code,
        CancellationToken ct);


    Task<PagedResponse<CourseResponseDto>> GetAllCoursesAsync(
        CancellationToken ct);


    Task InvalidateCourseCacheAsync(
        CancellationToken ct);
}