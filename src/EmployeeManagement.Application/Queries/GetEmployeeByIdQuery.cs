using EmployeeManagement.Application.DTOs;
using MediatR;

namespace EmployeeManagement.Application.Queries;

public sealed record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeDto>;
