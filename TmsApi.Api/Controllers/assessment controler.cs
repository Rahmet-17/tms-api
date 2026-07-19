using Microsoft.AspNetCore.Mvc;
using TmsApi.Domain.Entities;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssessmentsController : ControllerBase
{
    private readonly IAssessmentService _service;

    public AssessmentsController(IAssessmentService service)
    {
        _service = service;
    }

    // GET: api/assessments
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Assessment>>> GetAll()
    {
        var assessments = await _service.GetAllAsync();

        return Ok(assessments);
    }

    // GET: api/assessments/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Assessment>> GetById(int id)
    {
        var assessment = await _service.GetByIdAsync(id);

        if (assessment == null)
            return NotFound();

        return Ok(assessment);
    }

    // POST: api/assessments
    [HttpPost]
    public async Task<ActionResult<Assessment>> Create(
        string title,
        decimal maxScore,
        decimal weight,
        int courseId)
    {
        var assessment = await _service.CreateAsync(
            title,
            maxScore,
            weight,
            courseId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = assessment.Id },
            assessment);
    }

    // DELETE: api/assessments/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}