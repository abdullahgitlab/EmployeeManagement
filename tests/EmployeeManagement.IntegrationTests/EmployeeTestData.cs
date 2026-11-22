using Bogus;
using EmployeeManagement.Application.Commands;

namespace EmployeeManagement.IntegrationTests;

public static class EmployeeTestData
{
    public static CreateEmployeeCommand CreateCommand(string? department = null)
    {
        var faker = new Faker();
        return new CreateEmployeeCommand(
            faker.Name.FirstName(),
            faker.Name.LastName(),
            faker.Internet.Email(),
            faker.Phone.PhoneNumber(),
            department ?? faker.Commerce.Department(),
            faker.Name.JobTitle(),
            faker.Random.Decimal(1_000, 20_000),
            DateOnly.FromDateTime(faker.Date.Past(5)));
    }
}
