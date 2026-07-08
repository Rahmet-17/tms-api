using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;
using TmsApi.Interfaces;

namespace TmsApi.Services;

public class CourseService(
    TmsDbContext context,
    ILogger<CourseService> logger) : ICourseService
{
    // GET BY ID
    public Task<CourseResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        return context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count
            ))
            .FirstOrDefaultAsync(ct);
    }

    // CHECK IF COURSE CODE EXISTS
    public Task<bool> CodeExistsAsync(
        string code,
        CancellationToken ct)
    {
        return context.Courses
            .AsNoTracking()
            .AnyAsync(c => c.Code == code, ct);
    }

    // CREATE COURSE
    public async Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };

        context.Courses.Add(course);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created course {CourseId}",
            course.Id);

        return (await GetByIdAsync(course.Id, ct))!;
    }

    // GET COURSES WITH PAGINATION + SEARCH + SORT
    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct)
    {
        // Start query
        IQueryable<Course> query = context.Courses
            .AsNoTracking();

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
                EF.Functions.ILike(
                    c.Title,
                    $"%{request.Search}%")
                ||
                EF.Functions.ILike(
                    c.Code,
                    $"%{request.Search}%"));
        }

        //Count BEFORE pagination
        var totalCount = await query.CountAsync(ct);

        // Sorting
        IQueryable<Course> sortedQuery;

        switch (request.OrderBy)
        {
            case "Code":
                sortedQuery = request.Descending
                    ? query.OrderByDescending(c => c.Code)
                    : query.OrderBy(c => c.Code);
                break;

            case "MaxCapacity":
                sortedQuery = request.Descending
                    ? query.OrderByDescending(c => c.MaxCapacity)
                    : query.OrderBy(c => c.MaxCapacity);
                break;

            case "Title":
            default:
                sortedQuery = request.Descending
                    ? query.OrderByDescending(c => c.Title)
                    : query.OrderBy(c => c.Title);
                break;
        }

        // Apply pagination + DTO conversion
        var items = await sortedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count
            ))
            .ToListAsync(ct);

        //Return result
        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}