using MediatR;
using TmsApi.Application.Common;

namespace TmsApi.Application.Enrollments.Commands;

public record ApproveEnrollmentCommand(
    int EnrollmentId
)
    : IRequest<Result<EnrollmentApproved, EnrollmentError>>;


public record EnrollmentApproved(
    int EnrollmentId
);