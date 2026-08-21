using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/grades")]
public class GradesController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public GradesController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitGrade(
        SubmitGradeRequest request,
        CancellationToken ct)
    {
        var enrollment = await _enrollmentService
            .GetByStudentAndCourseAsync(
                request.StudentId,
                request.CourseId,
                ct);

        if (enrollment is null)
        {
            return NotFound(new
            {
                message = "Enrollment not found."
            });
        }

        enrollment.Grade = request.Score;

        await _enrollmentService.UpdateAsync(
            enrollment,
            ct);

        return Ok(new
        {
            studentId = enrollment.StudentId,
            courseId = enrollment.CourseId,
            score = enrollment.Grade
        });
    }
}