using MediatR;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Enrollments.Queries;

public class GetAllEnrollmentsHandler 
    : IRequestHandler<GetAllEnrollmentsQuery, IReadOnlyList<EnrollmentResponseDto>>
{
    private readonly IEnrollmentService enrollmentService;


    public GetAllEnrollmentsHandler(
        IEnrollmentService enrollmentService)
    {
        this.enrollmentService = enrollmentService;
    }


    public async Task<IReadOnlyList<EnrollmentResponseDto>> Handle(
        GetAllEnrollmentsQuery request,
        CancellationToken ct)
    {
        return await enrollmentService.GetAllAsync(ct);
    }
}