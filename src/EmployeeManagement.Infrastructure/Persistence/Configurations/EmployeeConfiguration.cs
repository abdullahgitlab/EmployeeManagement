using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagement.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(employee => employee.Id);
        builder.Property(employee => employee.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(employee => employee.LastName).HasMaxLength(100).IsRequired();
        builder.Property(employee => employee.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(employee => employee.Email).IsUnique();
        builder.Property(employee => employee.PhoneNumber).HasMaxLength(30).IsRequired();
        builder.Property(employee => employee.Department).HasMaxLength(100).IsRequired();
        builder.Property(employee => employee.Designation).HasMaxLength(100).IsRequired();
        builder.Property(employee => employee.Salary).HasPrecision(18, 2);
        builder.Property(employee => employee.DateOfJoining).HasColumnType("date");
    }
}
