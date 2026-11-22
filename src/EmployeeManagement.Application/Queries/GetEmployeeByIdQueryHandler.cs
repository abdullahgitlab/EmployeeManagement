using AutoMapper;
using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Interfaces;
using MediatR;

namespace EmployeeManagement.Application.Queries;

public sealed class GetEmployeeByIdQueryHandler(IEmployeeRepository repository, IMapper mapper)
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Employee '{request.Id}' was not found.");
        return mapper.Map<EmployeeDto>(employee);
    }
}
