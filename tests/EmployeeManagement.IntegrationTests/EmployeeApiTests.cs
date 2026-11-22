using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EmployeeManagement.Application.Commands;
using EmployeeManagement.Application.DTOs;
using FluentAssertions;

namespace EmployeeManagement.IntegrationTests;

public sealed class EmployeeApiTests
{
    [Fact]
    public async Task PostEmployees_ValidRequest_ReturnsCreatedAndLocation()
    {
        using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/employees", EmployeeTestData.CreateCommand());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var created = await response.Content.ReadFromJsonAsync<CreatedEmployee>();
        created!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PostEmployees_InvalidPayload_ReturnsProblemDetails400()
    {
        using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        var command = EmployeeTestData.CreateCommand() with { FirstName = string.Empty };

        using var response = await client.PostAsJsonAsync("/api/employees", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var problem = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        problem.RootElement.GetProperty("status").GetInt32().Should().Be(400);
        problem.RootElement.GetProperty("title").GetString().Should().Be("Validation failed");
        problem.RootElement.GetProperty("errors").TryGetProperty("FirstName", out _).Should().BeTrue();
    }

    [Fact]
    public async Task PostEmployees_DuplicateEmail_ReturnsProblemDetails409()
    {
        using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        var command = EmployeeTestData.CreateCommand();
        using var first = await client.PostAsJsonAsync("/api/employees", command);
        using var duplicate = await client.PostAsJsonAsync("/api/employees", command);

        first.StatusCode.Should().Be(HttpStatusCode.Created);
        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
        using var problem = await JsonDocument.ParseAsync(await duplicate.Content.ReadAsStreamAsync());
        problem.RootElement.GetProperty("status").GetInt32().Should().Be(409);
        problem.RootElement.GetProperty("title").GetString().Should().Be("Conflict");
    }

    [Fact]
    public async Task GetEmployeeById_ExistingId_ReturnsEmployeeDto()
    {
        using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        var command = EmployeeTestData.CreateCommand();
        var id = await CreateEmployeeAsync(client, command);

        var employee = await client.GetFromJsonAsync<EmployeeDto>($"/api/employees/{id}");

        employee.Should().NotBeNull();
        employee!.Id.Should().Be(id);
        employee.Email.Should().Be(command.Email);
        employee.FirstName.Should().Be(command.FirstName);
    }

    [Fact]
    public async Task GetEmployeeById_UnknownId_ReturnsProblemDetails404()
    {
        using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync($"/api/employees/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var problem = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        problem.RootElement.GetProperty("status").GetInt32().Should().Be(404);
        problem.RootElement.GetProperty("title").GetString().Should().Be("Resource not found");
    }

    [Fact]
    public async Task GetEmployees_PaginationSearchAndDepartment_ReturnExpectedPage()
    {
        using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        var first = EmployeeTestData.CreateCommand("Engineering") with { FirstName = "Quinn" };
        await CreateEmployeeAsync(client, first);
        await CreateEmployeeAsync(client, EmployeeTestData.CreateCommand("Engineering") with { FirstName = "Quinn" });
        await CreateEmployeeAsync(client, EmployeeTestData.CreateCommand("Finance") with { FirstName = "Quinn" });

        var page = await client.GetFromJsonAsync<PagedResult<EmployeeDto>>(
            "/api/employees?pageNumber=2&pageSize=1&search=Quinn&department=Engineering");

        page.Should().NotBeNull();
        page!.Items.Should().HaveCount(1);
        page.PageNumber.Should().Be(2);
        page.PageSize.Should().Be(1);
        page.TotalCount.Should().Be(2);
        page.Items[0].Department.Should().Be("Engineering");

        var emailSearch = await client.GetFromJsonAsync<PagedResult<EmployeeDto>>(
            $"/api/employees?search={Uri.EscapeDataString(first.Email)}");
        emailSearch!.TotalCount.Should().Be(1);
        emailSearch.Items[0].Email.Should().Be(first.Email);
    }

    [Fact]
    public async Task PutEmployee_ExistingId_ReturnsNoContent()
    {
        using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        var id = await CreateEmployeeAsync(client, EmployeeTestData.CreateCommand());
        var command = EmployeeTestData.CreateCommand();
        var update = new UpdateEmployeeCommand(id, command.FirstName, command.LastName, command.Email,
            command.PhoneNumber, command.Department, command.Designation, command.Salary, command.DateOfJoining);

        using var response = await client.PutAsJsonAsync($"/api/employees/{id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PutEmployee_UnknownId_ReturnsProblemDetails404()
    {
        using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        var command = EmployeeTestData.CreateCommand();
        var update = new UpdateEmployeeCommand(Guid.NewGuid(), command.FirstName, command.LastName, command.Email,
            command.PhoneNumber, command.Department, command.Designation, command.Salary, command.DateOfJoining);

        using var response = await client.PutAsJsonAsync($"/api/employees/{update.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var problem = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        problem.RootElement.GetProperty("status").GetInt32().Should().Be(404);
    }

    [Fact]
    public async Task PutEmployee_InvalidPayload_ReturnsProblemDetails400()
    {
        using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        var id = await CreateEmployeeAsync(client, EmployeeTestData.CreateCommand());
        var command = EmployeeTestData.CreateCommand();
        var update = new UpdateEmployeeCommand(id, string.Empty, command.LastName, command.Email,
            command.PhoneNumber, command.Department, command.Designation, 0, command.DateOfJoining);

        using var response = await client.PutAsJsonAsync($"/api/employees/{id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var problem = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        problem.RootElement.GetProperty("status").GetInt32().Should().Be(400);
        problem.RootElement.GetProperty("errors").TryGetProperty("FirstName", out _).Should().BeTrue();
        problem.RootElement.GetProperty("errors").TryGetProperty("Salary", out _).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteEmployee_ExistingId_SoftDeletesAndExcludesFromReads()
    {
        using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        var id = await CreateEmployeeAsync(client, EmployeeTestData.CreateCommand());

        using var response = await client.DeleteAsync($"/api/employees/{id}");
        var list = await client.GetFromJsonAsync<PagedResult<EmployeeDto>>("/api/employees");
        using var getResponse = await client.GetAsync($"/api/employees/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        list!.TotalCount.Should().Be(0);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteEmployee_UnknownId_ReturnsProblemDetails404()
    {
        using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();

        using var response = await client.DeleteAsync($"/api/employees/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var problem = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        problem.RootElement.GetProperty("status").GetInt32().Should().Be(404);
    }

    private static async Task<EmployeeApiFactory> CreateFactoryAsync()
    {
        var factory = new EmployeeApiFactory();
        await factory.InitializeDatabaseAsync();
        return factory;
    }

    private static async Task<Guid> CreateEmployeeAsync(HttpClient client, CreateEmployeeCommand command)
    {
        using var response = await client.PostAsJsonAsync("/api/employees", command);
        response.EnsureSuccessStatusCode();
        var employee = await response.Content.ReadFromJsonAsync<CreatedEmployee>();
        return employee!.Id;
    }

    private sealed record CreatedEmployee(Guid Id);
}
