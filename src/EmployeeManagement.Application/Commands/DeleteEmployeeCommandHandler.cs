using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Interfaces;
using MediatR;

namespace EmployeeManagement.Application.Commands;

public sealed class DeleteEmployeeCommandHandler(IEmployeeRepository repository)
    : IRequestHandler<DeleteEmployeeCommand, Unit>
{
    public async Task<Unit> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Employee '{request.Id}' was not found.");
        employee.IsActive = false;
        await repository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
