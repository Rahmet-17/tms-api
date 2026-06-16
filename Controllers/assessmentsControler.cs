using Microsoft.AspNetCore.Mvc;
using TmsCore.Interfaces;
using TmsCore.Models;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/assessments")]
public class AssessmentsController : ControllerBase
{
    private readonly IAssessmentService _service;

    public AssessmentsController(IAssessmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_service.GetAll());
    }

    [HttpGet("student/{studentId}")]
    public IActionResult GetByStudent(string studentId)
    {
        return Ok(_service.GetByStudent(studentId));
    }

    [HttpGet("course/{courseCode}")]
    public IActionResult GetByCourse(string courseCode)
    {
        return Ok(_service.GetByCourse(courseCode));
    }

    [HttpGet("average/{studentId}")]
    public IActionResult GetAverage(string studentId)
    {
        return Ok(_service.GetAverageScore(studentId));

    }
    [HttpGet("{id}")]
public IActionResult GetById(string id)
{
    var result = _service.GetById(id);
    return result is null ? NotFound() : Ok(result);
}

[HttpDelete("{id}")]
public IActionResult Delete(string id)
{
    var result = _service.Delete(id);
    return result ? NoContent() : NotFound();
}
    [HttpPost]
    public IActionResult Create([FromBody] Assessment assessment)
    {
        _service.AddAssessment(assessment);
        return Ok(assessment);
    }
}