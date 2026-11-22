namespace EmployeeManagement.Application.DTOs;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount);
