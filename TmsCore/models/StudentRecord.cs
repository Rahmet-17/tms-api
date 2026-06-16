namespace TmsCore.Models;

public record StudentRecord(
    string Id,
    string Name,
    DateTime CreatedAt,
    double? Gpa);