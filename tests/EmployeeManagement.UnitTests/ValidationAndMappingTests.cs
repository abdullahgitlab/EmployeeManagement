using AutoMapper;
using EmployeeManagement.Application.Behaviors;
using EmployeeManagement.Application.Commands;
using EmployeeManagement.Application.Mapping;
using EmployeeManagement.Application.Validators;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.UnitTests.TestData;
using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace EmployeeManagement.UnitTests;

public sealed class ValidationAndMappingTests
{
    private static readonly DateTimeOffset Now = new(2025, 5, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly FakeTimeProvider _timeProvider = new(Now);

    [Fact]
    public void CreateEmployeeCommandValidator_ValidCommand_HasNoErrors()
    {
        new CreateEmployeeCommandValidator(_timeProvider).TestValidate(EmployeeTestData.CreateCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateEmployeeCommandValidator_EmptyFirstName_HasError(string name)
    {
        var result = new CreateEmployeeCommandValidator(_timeProvider).TestValidate(EmployeeTestData.CreateCommand() with { FirstName = name });
        result.ShouldHaveValidationErrorFor(command => command.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateEmployeeCommandValidator_EmptyLastName_HasError(string name)
    {
        var result = new CreateEmployeeCommandValidator(_timeProvider).TestValidate(EmployeeTestData.CreateCommand() with { LastName = name });
        result.ShouldHaveValidationErrorFor(command => command.LastName);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public void CreateEmployeeCommandValidator_InvalidEmail_HasError(string email)
    {
        var result = new CreateEmployeeCommandValidator(_timeProvider).TestValidate(EmployeeTestData.CreateCommand() with { Email = email });
        result.ShouldHaveValidationErrorFor(command => command.Email);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateEmployeeCommandValidator_NonPositiveSalary_HasError(int salary)
    {
        var result = new CreateEmployeeCommandValidator(_timeProvider).TestValidate(EmployeeTestData.CreateCommand() with { Salary = salary });
        result.ShouldHaveValidationErrorFor(command => command.Salary);
    }

    [Theory]
    [InlineData(nameof(CreateEmployeeCommand.PhoneNumber))]
    [InlineData(nameof(CreateEmployeeCommand.Department))]
    [InlineData(nameof(CreateEmployeeCommand.Designation))]
    public void CreateEmployeeCommandValidator_EmptyRequiredField_HasError(string field)
    {
        var command = EmployeeTestData.CreateCommand();
        command = field switch
        {
            nameof(CreateEmployeeCommand.PhoneNumber) => command with { PhoneNumber = string.Empty },
            nameof(CreateEmployeeCommand.Department) => command with { Department = string.Empty },
            nameof(CreateEmployeeCommand.Designation) => command with { Designation = string.Empty },
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };

        new CreateEmployeeCommandValidator(_timeProvider).TestValidate(command).Errors
            .Should().Contain(error => error.PropertyName == field);
    }

    [Fact]
    public void CreateEmployeeCommandValidator_FutureJoiningDate_HasError()
    {
        var command = EmployeeTestData.CreateCommand() with { DateOfJoining = DateOnly.FromDateTime(Now.AddDays(1).UtcDateTime) };
        new CreateEmployeeCommandValidator(_timeProvider).TestValidate(command).ShouldHaveValidationErrorFor(item => item.DateOfJoining);
    }

    [Fact]
    public void UpdateEmployeeCommandValidator_ValidCommand_HasNoErrors()
    {
        var create = EmployeeTestData.CreateCommand();
        var command = new UpdateEmployeeCommand(Guid.NewGuid(), create.FirstName, create.LastName, create.Email,
            create.PhoneNumber, create.Department, create.Designation, create.Salary, create.DateOfJoining);
        new UpdateEmployeeCommandValidator(_timeProvider).TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", nameof(UpdateEmployeeCommand.FirstName))]
    [InlineData(" ", nameof(UpdateEmployeeCommand.FirstName))]
    [InlineData("", nameof(UpdateEmployeeCommand.LastName))]
    [InlineData(" ", nameof(UpdateEmployeeCommand.LastName))]
    public void UpdateEmployeeCommandValidator_EmptyName_HasError(string name, string field)
    {
        var create = EmployeeTestData.CreateCommand();
        var command = new UpdateEmployeeCommand(Guid.NewGuid(), create.FirstName, create.LastName, create.Email,
            create.PhoneNumber, create.Department, create.Designation, create.Salary, create.DateOfJoining);
        command = field == nameof(UpdateEmployeeCommand.FirstName)
            ? command with { FirstName = name }
            : command with { LastName = name };

        new UpdateEmployeeCommandValidator(_timeProvider).TestValidate(command).Errors
            .Should().Contain(error => error.PropertyName == field);
    }

    [Theory]
    [InlineData(nameof(UpdateEmployeeCommand.Id))]
    [InlineData(nameof(UpdateEmployeeCommand.PhoneNumber))]
    [InlineData(nameof(UpdateEmployeeCommand.Department))]
    [InlineData(nameof(UpdateEmployeeCommand.Designation))]
    public void UpdateEmployeeCommandValidator_EmptyRequiredField_HasError(string field)
    {
        var create = EmployeeTestData.CreateCommand();
        var command = new UpdateEmployeeCommand(Guid.NewGuid(), create.FirstName, create.LastName, create.Email,
            create.PhoneNumber, create.Department, create.Designation, create.Salary, create.DateOfJoining);
        command = field switch
        {
            nameof(UpdateEmployeeCommand.Id) => command with { Id = Guid.Empty },
            nameof(UpdateEmployeeCommand.PhoneNumber) => command with { PhoneNumber = string.Empty },
            nameof(UpdateEmployeeCommand.Department) => command with { Department = string.Empty },
            nameof(UpdateEmployeeCommand.Designation) => command with { Designation = string.Empty },
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };

        new UpdateEmployeeCommandValidator(_timeProvider).TestValidate(command).Errors
            .Should().Contain(error => error.PropertyName == field);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    public void UpdateEmployeeCommandValidator_InvalidEmail_HasError(string email)
    {
        var create = EmployeeTestData.CreateCommand();
        var command = new UpdateEmployeeCommand(Guid.NewGuid(), create.FirstName, create.LastName, email,
            create.PhoneNumber, create.Department, create.Designation, create.Salary, create.DateOfJoining);
        new UpdateEmployeeCommandValidator(_timeProvider).TestValidate(command).ShouldHaveValidationErrorFor(item => item.Email);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateEmployeeCommandValidator_NonPositiveSalary_HasError(int salary)
    {
        var create = EmployeeTestData.CreateCommand();
        var command = new UpdateEmployeeCommand(Guid.NewGuid(), create.FirstName, create.LastName, create.Email,
            create.PhoneNumber, create.Department, create.Designation, salary, create.DateOfJoining);
        new UpdateEmployeeCommandValidator(_timeProvider).TestValidate(command).ShouldHaveValidationErrorFor(item => item.Salary);
    }

    [Fact]
    public void UpdateEmployeeCommandValidator_FutureJoiningDate_HasError()
    {
        var create = EmployeeTestData.CreateCommand();
        var command = new UpdateEmployeeCommand(Guid.NewGuid(), create.FirstName, create.LastName, create.Email,
            create.PhoneNumber, create.Department, create.Designation, create.Salary, DateOnly.FromDateTime(Now.AddDays(1).UtcDateTime));
        new UpdateEmployeeCommandValidator(_timeProvider).TestValidate(command).ShouldHaveValidationErrorFor(item => item.DateOfJoining);
    }

    [Fact]
    public async Task ValidationBehavior_ValidRequest_CallsNext()
    {
        var command = EmployeeTestData.CreateCommand();
        var behavior = new ValidationBehavior<CreateEmployeeCommand, Guid>([new CreateEmployeeCommandValidator(_timeProvider)]);
        var expected = Guid.NewGuid();
        var nextCalled = false;
        MediatR.RequestHandlerDelegate<Guid> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(expected);
        };

        var result = await behavior.Handle(command, next, CancellationToken.None);

        result.Should().Be(expected);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ValidationBehavior_InvalidRequest_ThrowsAndDoesNotCallNext()
    {
        var command = EmployeeTestData.CreateCommand() with { FirstName = string.Empty };
        var behavior = new ValidationBehavior<CreateEmployeeCommand, Guid>([new CreateEmployeeCommandValidator(_timeProvider)]);
        var repository = new Mock<EmployeeManagement.Application.Interfaces.IEmployeeRepository>();
        var handler = new EmployeeManagement.Application.Commands.CreateEmployeeCommandHandler(repository.Object, _timeProvider);
        var nextCalled = false;
        MediatR.RequestHandlerDelegate<Guid> next = () =>
        {
            nextCalled = true;
            return handler.Handle(command, CancellationToken.None);
        };

        var act = () => behavior.Handle(command, next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        nextCalled.Should().BeFalse();
        repository.Verify(repo => repo.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void EmployeeMappingProfile_ValidConfiguration_MapsAllFields()
    {
        var configuration = new MapperConfiguration(config => config.AddProfile<EmployeeMappingProfile>());
        configuration.AssertConfigurationIsValid();
        var employee = EmployeeTestData.CreateEmployee();

        var dto = configuration.CreateMapper().Map<EmployeeManagement.Application.DTOs.EmployeeDto>(employee);

        dto.Should().BeEquivalentTo(employee);
    }
}
