using MediatR;

namespace EmployeeManagement.Application.Commands;

public sealed record DeleteEmployeeCommand(Guid Id) : IRequest<Unit>;
