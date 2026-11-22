using AutoMapper;
using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Application.Mapping;
using EmployeeManagement.Application.Queries;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.UnitTests.TestData;
using FluentAssertions;
using Moq;

namespace EmployeeManagement.UnitTests;

public sealed class QueryHandlerTests
{
    private readonly Mock<IEmployeeRepository> _repository = new();
    private readonly IMapper _mapper = new MapperConfiguration(configuration => configuration.AddProfile<EmployeeMappingProfile>()).CreateMapper();

    [Fact]
    public async Task GetEmployeeByIdQueryHandler_ExistingEmployee_ReturnsMappedDto()
    {
        var employee = EmployeeTestData.CreateEmployee();
        _repository.Setup(repository => repository.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(employee);

        var result = await new GetEmployeeByIdQueryHandler(_repository.Object, _mapper)
            .Handle(new GetEmployeeByIdQuery(employee.Id), CancellationToken.None);

        result.Should().BeEquivalentTo(employee);
    }

    [Fact]
    public async Task GetEmployeeByIdQueryHandler_UnknownEmployee_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Employee?)null);

        var act = () => new GetEmployeeByIdQueryHandler(_repository.Object, _mapper)
            .Handle(new GetEmployeeByIdQuery(id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAllEmployeesQueryHandler_PaginationAndFilters_ReturnsMappedPageAndTotal()
    {
        var employees = new[] { EmployeeTestData.CreateEmployee(), EmployeeTestData.CreateEmployee() };
        _repository.Setup(repository => repository.GetPageAsync(2, 5, "Ada", "Engineering", It.IsAny<CancellationToken>())).ReturnsAsync(employees);
        _repository.Setup(repository => repository.CountAsync("Ada", "Engineering", It.IsAny<CancellationToken>())).ReturnsAsync(12);

        var result = await new GetAllEmployeesQueryHandler(_repository.Object, _mapper)
            .Handle(new GetAllEmployeesQuery(2, 5, "Ada", "Engineering"), CancellationToken.None);

        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(12);
        result.Items.Should().HaveCount(2);
        result.Items[0].Id.Should().Be(employees[0].Id);
        _repository.Verify(repository => repository.GetPageAsync(2, 5, "Ada", "Engineering", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllEmployeesQueryHandler_PageSizeOutOfRange_ClampsToMaximum()
    {
        _repository.Setup(repository => repository.GetPageAsync(1, 100, null, null, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _repository.Setup(repository => repository.CountAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await new GetAllEmployeesQueryHandler(_repository.Object, _mapper)
            .Handle(new GetAllEmployeesQuery(0, 1000), CancellationToken.None);

        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(100);
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
}
