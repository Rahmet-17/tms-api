using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;

namespace TmsApi.Infrastructure.Services;

public class CachedCourseService(
    HybridCache cache,
    ICourseService courseService,
    ILogger<CachedCourseService> logger)
    : ICachedCourseService
{

    public async Task<CourseResponseDto> GetCourseAsync(
        string code,
        CancellationToken ct)
    {
        var key = CacheKeys.Course(code);

        var dbHit = false;

        var dto = await cache.GetOrCreateAsync(
            key,
            (courseService, code),

            async (state, token) =>
            {
                dbHit = true;

                logger.LogInformation(
                    "Cache MISS for {Key} fetching from service",
                    key);


                var course =
                    await state.courseService.GetByCodeAsync(
                        state.code,
                        token);


                if (course == null)
                {
                    throw new Exception(
                        $"Course {state.code} not found.");
                }


                return new CourseResponseDto(
                    course.Id,
                    course.Title,
                    course.Code,
                    course.MaxCapacity,
                    course.Enrollments.Count);
            },

            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct);


        if (!dbHit)
        {
            logger.LogInformation(
                "Cache HIT for {Key}",
                key);
        }


        return dto;
    }



    public async Task<PagedResponse<CourseResponseDto>> GetAllCoursesAsync(
        CancellationToken ct)
    {
        var key = CacheKeys.CoursesAll;

        var dbHit = false;


        var result = await cache.GetOrCreateAsync(
            key,
            courseService,

            async (state, token) =>
            {
                dbHit = true;

                logger.LogInformation(
                    "Cache MISS for {Key} fetching from service",
                    key);


                return await state.GetCoursesAsync(
                    new PagedRequest(),
                    token);
            },

            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct);



        if (!dbHit)
        {
            logger.LogInformation(
                "Cache HIT for {Key}",
                key);
        }


        return result;
    }



    public async Task InvalidateCourseCacheAsync(
        CancellationToken ct)
    {
        logger.LogInformation(
            "Invalidating cache tag {Tag}",
            CacheKeys.CoursesTag);


        await cache.RemoveByTagAsync(
            CacheKeys.CoursesTag,
            ct);
    }
}