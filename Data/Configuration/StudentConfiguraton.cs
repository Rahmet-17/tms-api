using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations
{

    
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.ToTable("Students");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.RegistrationNumber)
                .IsRequired();

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.GPA)
                .IsRequired();
        }

        
    }
}