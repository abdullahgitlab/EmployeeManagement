using AutoMapper;
using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Interfaces;
using MediatR;

namespace EmployeeManagement.Application.Queries;

public sealed class GetAllEmployeesQueryHandler(IEmployeeRepository repository, IMapper mapper)
    : IRequestHandler<GetAllEmployeesQuery, PagedResult<EmployeeDto>>
{
    public async Task<PagedResult<EmployeeDto>> Handle(GetAllEmployeesQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var employees = await repository.GetPageAsync(pageNumber, pageSize, request.Search, request.Department, cancellationToken);
        var totalCount = await repository.CountAsync(request.Search, request.Department, cancellationToken);
        return new PagedResult<EmployeeDto>(mapper.Map<IReadOnlyList<EmployeeDto>>(employees), pageNumber, pageSize, totalCount);
    }
}
