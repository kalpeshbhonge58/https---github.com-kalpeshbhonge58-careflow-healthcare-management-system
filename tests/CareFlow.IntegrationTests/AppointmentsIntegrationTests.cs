using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareFlow.Application.DTOs.Appointment;
using CareFlow.Application.DTOs.Auth;
using CareFlow.Shared.Models;

namespace CareFlow.IntegrationTests;

public class AppointmentsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AppointmentsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateAppointment_Authorized_BooksSuccessfully()
    {
        var client = await CreateAuthorizedClientAsync();

        var futureDate = DateTime.UtcNow.Date.AddDays(7);
        var request = new CreateAppointmentRequest
        {
            PatientId = 1,
            DoctorId = 1,
            AppointmentDate = futureDate,
            StartTime = new TimeSpan(16, 0, 0),
            EndTime = new TimeSpan(16, 30, 0),
            Reason = "Integration test booking"
        };

        var response = await client.PostAsJsonAsync("/api/appointments", request);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AppointmentResponse>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.Equal("Scheduled", body.Data!.Status);
        Assert.Equal("Integration test booking", body.Data.Reason);
    }

    [Fact]
    public async Task GetAppointments_Authorized_ReturnsPagedList()
    {
        var client = await CreateAuthorizedClientAsync();

        var response = await client.GetAsync("/api/appointments?pageNumber=1&pageSize=10");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<AppointmentResponse>>>();
        Assert.NotNull(body?.Data);
        Assert.NotEmpty(body.Data!.Items);
    }

    [Fact]
    public async Task CreateAppointment_Unauthorized_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var request = new CreateAppointmentRequest
        {
            PatientId = 1,
            DoctorId = 1,
            AppointmentDate = DateTime.UtcNow.Date.AddDays(5),
            StartTime = new TimeSpan(12, 0, 0),
            EndTime = new TimeSpan(12, 30, 0)
        };

        var response = await client.PostAsJsonAsync("/api/appointments", request);

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
