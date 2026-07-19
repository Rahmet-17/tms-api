namespace TmsApi.Domain.Entities;

public class Student
{
    // Surrogate primary key
    public int Id { get; set; }

    // Natural key
    public required string RegistrationNumber { get; set; }

    public required string Name { get; set; }

    public decimal GPA { get; set; }

    public bool IsActive { get; set; } = true;

    // Soft delete
    public bool IsDeleted { get; set; } = false;

    // Concurrency token 
    // PostgreSQL maps this to xmin automatically
    public uint Version { get; set; }

    // Navigation property
    public ICollection<Enrollment> Enrollments { get; set; }
        = new List<Enrollment>();
}