using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;
using TmsApi.Application.Hubs;
using TmsApi.Api.Hubs;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[Route("api/enrollments")]
[ApiVersion("2.0")]
[Tags("Enrollments")]
public class EnrollmentsController(
    IMediator mediator,
    IHubContext<TmsHub, ITmsHubClient> hubContext)
    : ControllerBase
{
    // GET api/enrollments
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken ct)
    {
        var enrollments = await mediator.Send(
            new GetAllEnrollmentsQuery(),
            ct);

        return Ok(enrollments);
    }

    // POST api/enrollments
    [HttpPost]
    public async Task<IActionResult> Enroll(
        EnrollStudentCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            command,
            ct);

        return await result.Match<Task<IActionResult>>(
            onSuccess: async created =>
            {
                // Notify all connected Angular clients that
                // a new pending enrollment exists.
                await hubContext.Clients.All
                    .ReceiveEnrollmentStatusUpdated(
                        "new-enrollment",
                        "Pending");

                return CreatedAtAction(
                    nameof(GetSchedule),
                    new
                    {
                        studentId = created.StudentId
                    },
                    created);
            },

            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "course_not_found" =>
                        StatusCodes.Status404NotFound,

                    "course_full" or "already_enrolled" =>
                        StatusCodes.Status409Conflict,

                    _ =>
                        StatusCodes.Status400BadRequest
                };

                IActionResult response = Problem(
                    statusCode: status,
                    title: "Enrollment rejected",
                    detail: error.Message);

                return Task.FromResult(response);
            });
    }

    // GET api/enrollments/{studentId}/schedule
    [HttpGet("{studentId:int}/schedule")]
    public async Task<IActionResult> GetSchedule(
        int studentId,
        CancellationToken ct)
    {
        var schedule = await mediator.Send(
            new GetStudentScheduleQuery(studentId),
            ct);

        return Ok(schedule);
    }

    // POST api/enrollments/{id}/approve
    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(
        int id,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new ApproveEnrollmentCommand(id),
            ct);

        return await result.Match<Task<IActionResult>>(
            onSuccess: async approved =>
            {
                await hubContext.Clients.All
                    .ReceiveEnrollmentStatusUpdated(
                        approved.EnrollmentId.ToString(),
                        "Approved");

                return Ok(approved);
            },

            onFailure: error =>
            {
                IActionResult response = Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Approval rejected",
                    detail: error.Message);

                return Task.FromResult(response);
            });
    }
}