using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Repositories;

public sealed class EmployeeRepository(EmployeeManagementDbContext dbContext) : IEmployeeRepository
{
    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Employees.FirstOrDefaultAsync(employee => employee.Id == id && employee.IsActive, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, Guid? exceptEmployeeId, CancellationToken cancellationToken) =>
        dbContext.Employees.AnyAsync(employee => employee.Email == email &&
            (!exceptEmployeeId.HasValue || employee.Id != exceptEmployeeId.Value), cancellationToken);

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken) =>
        await dbContext.Employees.AddAsync(employee, cancellationToken);

    public async Task<IReadOnlyList<Employee>> GetPageAsync(int pageNumber, int pageSize, string? search, string? department, CancellationToken cancellationToken)
    {
        var query = Filtered(search, department);
        return await query.OrderBy(employee => employee.LastName).ThenBy(employee => employee.FirstName)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(string? search, string? department, CancellationToken cancellationToken) =>
        Filtered(search, department).CountAsync(cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictException("An employee with this email already exists.");
        }
    }

    private IQueryable<Employee> Filtered(string? search, string? department)
    {
        var query = dbContext.Employees.Where(employee => employee.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(employee => employee.FirstName.Contains(term) ||
                employee.LastName.Contains(term) || employee.Email.Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(employee => employee.Department == department.Trim());
        return query;
    }
}
