using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Entities;
using MediatR;

namespace EmployeeManagement.Application.Commands;

public sealed class CreateEmployeeCommandHandler(IEmployeeRepository repository, TimeProvider timeProvider)
    : IRequestHandler<CreateEmployeeCommand, Guid>
{
    public async Task<Guid> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        if (await repository.EmailExistsAsync(request.Email, null, cancellationToken))
            throw new ConflictException($"An employee with email '{request.Email}' already exists.");

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Department = request.Department.Trim(),
            Designation = request.Designation.Trim(),
            Salary = request.Salary,
            DateOfJoining = request.DateOfJoining,
            IsActive = true,
            CreatedAt = timeProvider.GetUtcNow()
        };

        await repository.AddAsync(employee, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return employee.Id;
    }
}
