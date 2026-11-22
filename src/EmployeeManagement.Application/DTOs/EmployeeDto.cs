namespace EmployeeManagement.Application.DTOs;

public sealed record EmployeeDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Department,
    string Designation,
    decimal Salary,
    DateOnly DateOfJoining,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
