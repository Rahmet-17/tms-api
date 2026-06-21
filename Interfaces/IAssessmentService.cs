using System.Collections.Generic;
using TmsCore.Models;

namespace TmsCore.Interfaces;

public interface IAssessmentService
{
    void AddAssessment(Assessment assessment);
    List<Assessment> GetAll();
    Assessment? GetById(string id);
    List<Assessment> GetByStudent(string studentId);
    List<Assessment> GetByCourse(string courseCode);
    decimal GetAverageScore(string studentId);
    bool Delete(string id);
}