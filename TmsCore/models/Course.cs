namespace TmsCore.Models;

public class Course
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int EnrolledCount { get; set; } = 0;
}