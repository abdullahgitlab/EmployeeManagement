using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Interfaces;
using MediatR;

namespace EmployeeManagement.Application.Commands;

public sealed class UpdateEmployeeCommandHandler(IEmployeeRepository repository, TimeProvider timeProvider)
    : IRequestHandler<UpdateEmployeeCommand, Unit>
{
    public async Task<Unit> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Employee '{request.Id}' was not found.");

        if (await repository.EmailExistsAsync(request.Email, request.Id, cancellationToken))
            throw new ConflictException($"An employee with email '{request.Email}' already exists.");

        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.Email = request.Email.Trim();
        employee.PhoneNumber = request.PhoneNumber.Trim();
        employee.Department = request.Department.Trim();
        employee.Designation = request.Designation.Trim();
        employee.Salary = request.Salary;
        employee.DateOfJoining = request.DateOfJoining;
        employee.UpdatedAt = timeProvider.GetUtcNow();

        await repository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
