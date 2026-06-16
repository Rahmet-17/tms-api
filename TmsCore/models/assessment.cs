namespace TmsCore.Models;

public class Assessment
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string StudentId { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public decimal Score { get; set; }
}