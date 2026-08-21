using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Enrollments.Commands;

public class ApproveEnrollmentHandler(
    IEnrollmentService enrollmentService)
    : IRequestHandler<ApproveEnrollmentCommand, Result<EnrollmentApproved, EnrollmentError>>
{
    public async Task<Result<EnrollmentApproved, EnrollmentError>> Handle(
        ApproveEnrollmentCommand command,
        CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetByIdForUpdateAsync(
            command.EnrollmentId,
            ct);

        if (enrollment is null)
        {
            return Result<EnrollmentApproved, EnrollmentError>.Failure(
    new EnrollmentError(
        "enrollment_not_found",
        $"Enrollment {command.EnrollmentId} was not found."));
        }


        enrollment.Status = "Approved";

        await enrollmentService.UpdateAsync(
            enrollment,
            ct);


        return Result<EnrollmentApproved, EnrollmentError>.Success(
            new EnrollmentApproved(
                enrollment.Id));
    }
}