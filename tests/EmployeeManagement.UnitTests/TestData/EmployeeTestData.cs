using Bogus;
using EmployeeManagement.Application.Commands;
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.UnitTests.TestData;

public static class EmployeeTestData
{
    private static readonly Faker Faker = new();

    public static Employee CreateEmployee() => new Faker<Employee>()
        .RuleFor(employee => employee.Id, _ => Guid.NewGuid())
        .RuleFor(employee => employee.FirstName, faker => faker.Name.FirstName())
        .RuleFor(employee => employee.LastName, faker => faker.Name.LastName())
        .RuleFor(employee => employee.Email, faker => faker.Internet.Email())
        .RuleFor(employee => employee.PhoneNumber, faker => faker.Phone.PhoneNumber())
        .RuleFor(employee => employee.Department, faker => faker.Commerce.Department())
        .RuleFor(employee => employee.Designation, faker => faker.Name.JobTitle())
        .RuleFor(employee => employee.Salary, faker => faker.Random.Decimal(1_000, 20_000))
        .RuleFor(employee => employee.DateOfJoining, faker => DateOnly.FromDateTime(faker.Date.Past(5)))
        .RuleFor(employee => employee.IsActive, _ => true)
        .RuleFor(employee => employee.CreatedAt, faker => faker.Date.RecentOffset())
        .Generate();

    public static CreateEmployeeCommand CreateCommand() => new(
        Faker.Name.FirstName(),
        Faker.Name.LastName(),
        Faker.Internet.Email(),
        Faker.Phone.PhoneNumber(),
        Faker.Commerce.Department(),
        Faker.Name.JobTitle(),
        Faker.Random.Decimal(1_000, 20_000),
        new DateOnly(2020, 1, 1));
}
