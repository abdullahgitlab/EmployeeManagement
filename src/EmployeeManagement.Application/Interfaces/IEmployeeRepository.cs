using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, Guid? exceptEmployeeId, CancellationToken cancellationToken);
    Task AddAsync(Employee employee, CancellationToken cancellationToken);
    Task<IReadOnlyList<Employee>> GetPageAsync(int pageNumber, int pageSize, string? search, string? department, CancellationToken cancellationToken);
    Task<int> CountAsync(string? search, string? department, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
