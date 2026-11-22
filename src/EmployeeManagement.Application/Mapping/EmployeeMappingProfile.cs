using AutoMapper;
using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Mapping;

public sealed class EmployeeMappingProfile : Profile
{
    public EmployeeMappingProfile() => CreateMap<Employee, EmployeeDto>();
}
