using MediatR;
using TmsApi.Application.Dtos;

namespace TmsApi.Application.Enrollments.Queries;

public record GetAllEnrollmentsQuery 
    : IRequest<IReadOnlyList<EnrollmentResponseDto>>;