using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// Configureer de Azure Functions Web Application
builder.ConfigureFunctionsWebApplication();

// Voeg HttpClient toe voor gebruik in de Functions
builder.Services.AddHttpClient();

// Application Insights (optioneel, kan later worden ingeschakeld)
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
