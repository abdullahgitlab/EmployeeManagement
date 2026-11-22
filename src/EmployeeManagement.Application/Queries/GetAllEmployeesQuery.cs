using EmployeeManagement.Application.DTOs;
using MediatR;

namespace EmployeeManagement.Application.Queries;

public sealed record GetAllEmployeesQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? Search = null,
    string? Department = null) : IRequest<PagedResult<EmployeeDto>>;
