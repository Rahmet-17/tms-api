using TmsCore.Interfaces;
using TmsCore.Models;

namespace TmsCore.Services;

public class AssessmentService : IAssessmentService
{
    public bool Delete(string id)
{
    var item = _assessments.FirstOrDefault(x => x.Id == id);

    if (item == null)
        return false;

    _assessments.Remove(item);
    return true;
}
    public Assessment? GetById(string id)
{
    return _assessments.FirstOrDefault(x => x.Id == id);
}
    private readonly List<Assessment> _assessments = new();

    public void AddAssessment(Assessment assessment)
    {
        _assessments.Add(assessment);
    }

    public List<Assessment> GetAll()
    {
        return _assessments;
    }

    public List<Assessment> GetByStudent(string studentId)
    {
        return _assessments.Where(x => x.StudentId == studentId).ToList();
    }

    public List<Assessment> GetByCourse(string courseCode)
    {
        return _assessments.Where(x => x.CourseCode == courseCode).ToList();
    }

    public decimal GetAverageScore(string studentId)
    {
        var scores = _assessments
            .Where(x => x.StudentId == studentId)
            .Select(x => x.Score);

        return scores.Any() ? scores.Average() : 0;
    }
}