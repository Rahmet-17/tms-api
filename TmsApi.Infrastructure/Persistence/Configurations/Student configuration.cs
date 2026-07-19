using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        // Primary key
        builder.HasKey(s => s.Id);

        // Registration number must be unique
        builder.HasIndex(s => s.RegistrationNumber)
               .IsUnique();

        builder.Property(s => s.RegistrationNumber)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(s => s.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(s => s.GPA)
               .HasPrecision(3, 2);

        //  shadow property
        builder.Property<DateTime>("LastUpdated");

        // concurrency token
        builder.Property(s => s.Version)
               .IsRowVersion();

        //  soft delete filter
        builder.HasQueryFilter(s => !s.IsDeleted);

        // Relationships
        builder.HasMany(s => s.Enrollments)
               .WithOne(e => e.Student)
               .HasForeignKey(e => e.StudentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}