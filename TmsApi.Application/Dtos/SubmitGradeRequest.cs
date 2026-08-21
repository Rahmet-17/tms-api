namespace TmsApi.Application.Dtos;

public record SubmitGradeRequest(
    int StudentId,
    int CourseId,
    decimal Score
);