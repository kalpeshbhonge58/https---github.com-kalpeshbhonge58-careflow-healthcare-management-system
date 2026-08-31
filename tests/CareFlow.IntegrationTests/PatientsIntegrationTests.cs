using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareFlow.Application.DTOs.Auth;
using CareFlow.Application.DTOs.Patient;
using CareFlow.Domain.Enums;
using CareFlow.Shared.Models;

namespace CareFlow.IntegrationTests;

public class PatientsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PatientsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPatients_Authorized_ReturnsPagedList()
    {
        var client = await CreateAuthorizedClientAsync();

        var response = await client.GetAsync("/api/patients?pageNumber=1&pageSize=10");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<PatientResponse>>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.NotEmpty(body.Data!.Items);
    }

    [Fact]
    public async Task CreateAndUpdatePatient_Authorized_Succeeds()
    {
        var client = await CreateAuthorizedClientAsync();

        var createRequest = new CreatePatientRequest
        {
            Email = $"integration.patient.{Guid.NewGuid():N}@test.com",
            Password = "Password123",
            FirstName = "Integration",
            LastName = "Patient",
            DateOfBirth = new DateTime(1992, 6, 15),
            Gender = Gender.Female,
            PhoneNumber = "+1-555-9999"
        };

        var createResponse = await client.PostAsJsonAsync("/api/patients", createRequest);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<PatientResponse>>();
        Assert.NotNull(created?.Data);
        Assert.Equal("Integration", created.Data.FirstName);

        var updateRequest = new UpdatePatientRequest
        {
            FirstName = "Updated",
            LastName = "Patient",
            DateOfBirth = createRequest.DateOfBirth,
            Gender = Gender.Female
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/patients/{created.Data.Id}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        var updated = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<PatientResponse>>();
        Assert.Equal("Updated", updated?.Data?.FirstName);
    }

    [Fact]
    public async Task GetPatients_Unauthorized_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/patients");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync()
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "admin@careflow.com",
            Password = "Admin@123"
        });

        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Data!.Token);
        return client;
    }
}
