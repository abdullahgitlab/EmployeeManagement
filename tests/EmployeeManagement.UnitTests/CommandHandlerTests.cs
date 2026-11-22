using EmployeeManagement.Application.Commands;
using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.UnitTests.TestData;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace EmployeeManagement.UnitTests;

public sealed class CommandHandlerTests
{
    private readonly Mock<IEmployeeRepository> _repository = new();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 5, 1, 0, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task CreateEmployeeCommandHandler_ValidRequest_ReturnsNewId()
    {
        var command = EmployeeTestData.CreateCommand();
        Employee? added = null;
        _repository.Setup(repository => repository.EmailExistsAsync(command.Email, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repository.Setup(repository => repository.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Callback<Employee, CancellationToken>((employee, _) => added = employee).Returns(Task.CompletedTask);
        _repository.Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await new CreateEmployeeCommandHandler(_repository.Object, _timeProvider)
            .Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        added!.Id.Should().Be(result);
        added.CreatedAt.Should().Be(_timeProvider.GetUtcNow());
        _repository.Verify(repository => repository.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEmployeeCommandHandler_DuplicateEmail_ThrowsConflictException()
    {
        var command = EmployeeTestData.CreateCommand();
        _repository.Setup(repository => repository.EmailExistsAsync(command.Email, null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => new CreateEmployeeCommandHandler(_repository.Object, _timeProvider)
            .Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        _repository.Verify(repository => repository.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Never);
        _repository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeCommandHandler_ExistingEmployee_UpdatesFieldsAndUpdatedAt()
    {
        var employee = EmployeeTestData.CreateEmployee();
        var command = EmployeeTestData.CreateCommand();
        _repository.Setup(repository => repository.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(employee);
        _repository.Setup(repository => repository.EmailExistsAsync(command.Email, employee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repository.Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await new UpdateEmployeeCommandHandler(_repository.Object, _timeProvider).Handle(
            new UpdateEmployeeCommand(employee.Id, command.FirstName, command.LastName, command.Email,
                command.PhoneNumber, command.Department, command.Designation, command.Salary, command.DateOfJoining),
            CancellationToken.None);

        employee.FirstName.Should().Be(command.FirstName);
        employee.Email.Should().Be(command.Email);
        employee.UpdatedAt.Should().Be(_timeProvider.GetUtcNow());
        _repository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEmployeeCommandHandler_UnknownEmployee_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        var command = EmployeeTestData.CreateCommand();
        _repository.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Employee?)null);

        var act = () => new UpdateEmployeeCommandHandler(_repository.Object, _timeProvider).Handle(
            new UpdateEmployeeCommand(id, command.FirstName, command.LastName, command.Email,
                command.PhoneNumber, command.Department, command.Designation, command.Salary, command.DateOfJoining),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _repository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteEmployeeCommandHandler_ExistingEmployee_SoftDeletesEmployee()
    {
        var employee = EmployeeTestData.CreateEmployee();
        _repository.Setup(repository => repository.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(employee);
        _repository.Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await new DeleteEmployeeCommandHandler(_repository.Object).Handle(new DeleteEmployeeCommand(employee.Id), CancellationToken.None);

        employee.IsActive.Should().BeFalse();
        _repository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteEmployeeCommandHandler_UnknownEmployee_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Employee?)null);

        var act = () => new DeleteEmployeeCommandHandler(_repository.Object).Handle(new DeleteEmployeeCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _repository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
