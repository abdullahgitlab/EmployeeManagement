using MediatR;

namespace EmployeeManagement.Application.Commands;

public sealed record UpdateEmployeeCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Department,
    string Designation,
    decimal Salary,
    DateOnly DateOfJoining) : IRequest<Unit>;
