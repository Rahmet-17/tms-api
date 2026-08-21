using System.Threading.Channels;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Transcripts;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/transcripts")]
[ApiVersion("2.0")]
[ApiExplorerSettings(GroupName = "v2")]
public class TranscriptsController(
    Channel<TranscriptRequest> channel,
    ITranscriptStatusStore statusStore) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public async Task<IActionResult> RequestTranscript(
        [FromBody] TranscriptRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        // Check whether this request was already submitted
        // with the same idempotency key.
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing =
                await statusStore.GetReportIdForIdempotencyKeyAsync(
                    idempotencyKey,
                    ct);

            if (existing is not null)
            {
                var existingStatus =
                    await statusStore.GetAsync(existing, ct);

                return Accepted(
                    Url.Action(nameof(GetStatus), new { id = existing }),
                    existingStatus);
            }
        }

        // Create a new report ID.
        var reportId = Guid.NewGuid().ToString("N")[..12];

        // Create the initial Queued status.
        await statusStore.CreateAsync(
            reportId,
            request.StudentId,
            ct);

        // Save the idempotency key so future duplicate
        // requests return this same report.
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await statusStore.LinkIdempotencyKeyAsync(
                idempotencyKey,
                reportId,
                ct);
        }

        // Put the transcript request into the background queue.
        await channel.Writer.WriteAsync(
            request.WithReportId(reportId),
            ct);

        Response.Headers.RetryAfter = "5";

        // Get the newly-created Queued status.
        var status = await statusStore.GetAsync(
            reportId,
            ct);

        // 202 Accepted because the transcript is processed
        // asynchronously by TranscriptWorker.
        return Accepted(
            Url.Action(nameof(GetStatus), new { id = reportId }),
            status);
    }

    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetStatus(
        string id,
        CancellationToken ct)
    {
        var status = await statusStore.GetAsync(
            id,
            ct);

        return status is null
            ? NotFound(new ProblemDetails
            {
                Title = "Transcript not found",
                Detail = $"No transcript request with id '{id}'.",
                Status = StatusCodes.Status404NotFound
            })
            : Ok(status);
    }
}