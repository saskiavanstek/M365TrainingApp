using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TrainerFrontend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Voeg de appsettings.json configuratie toe
builder.Configuration.AddJsonFile("appsettings.json");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// Voeg HttpClient toe
builder.Services.AddScoped(sp => new HttpClient()); 

// Voeg IConfiguration toe als service
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Voeg AppDbContext toe als service met de connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();