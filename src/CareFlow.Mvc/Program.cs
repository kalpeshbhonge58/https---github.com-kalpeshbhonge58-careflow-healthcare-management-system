// CareFlow.Mvc — Admin reporting application.
// This MVC project consumes the CareFlow Web API (http://localhost:5000) for all data.
// Business logic lives in CareFlow.Application; MVC only handles presentation and HTTP calls.

using CareFlow.Infrastructure.Data;
using CareFlow.Mvc.Models;
using CareFlow.Mvc.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<CareFlowApiSettings>(
    builder.Configuration.GetSection(CareFlowApiSettings.SectionName));

builder.Services.AddControllersWithViews();

var apiBaseUrl = builder.Configuration.GetValue<string>("CareFlowApi:BaseUrl") ?? "http://localhost:5000";

builder.Services.AddHttpClient<ICareFlowApiClient, CareFlowApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Reports}/{action=Index}/{id?}");

app.Run();
