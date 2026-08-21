namespace TmsApi.Application.Dtos;

public class UpdateCourseRequest
{
    public string Title { get; set; } = string.Empty;

    public int? MaxCapacity { get; set; }
}