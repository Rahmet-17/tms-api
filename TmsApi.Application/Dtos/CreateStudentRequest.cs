namespace TmsApi.Application.Dtos;

public record CreateStudentRequest(
    string Name,
    string RegistrationNumber
);