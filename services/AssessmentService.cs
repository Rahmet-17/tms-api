using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Interfaces;

namespace TmsApi.Services;

public class AssessmentService : IAssessmentService
{
    private readonly TmsDbContext _context;

    public AssessmentService(TmsDbContext context)
    {
        _context = context;
    }

    public async Task<Assessment> CreateAsync(
        string title,
        decimal maxScore,
        decimal weight,
        int courseId)
    {
        var assessment = new Assessment
        {
            Title = title,
            MaxScore = maxScore,
            Weight = weight,
            CourseId = courseId
        };

        _context.Assessments.Add(assessment);

        await _context.SaveChangesAsync();

        return assessment;
    }

    public async Task<Assessment?> GetByIdAsync(int id)
    {
        return await _context.Assessments
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IReadOnlyList<Assessment>> GetAllAsync()
    {
        return await _context.Assessments.ToListAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var assessment = await _context.Assessments
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assessment == null)
            return false;

        _context.Assessments.Remove(assessment);

        await _context.SaveChangesAsync();

        return true;
    }
}