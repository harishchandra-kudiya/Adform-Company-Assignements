using Adfrom_CurrencyConversion.Interfaces;
using Adfrom_CurrencyConversion.Services;
using log4net.Config;
using log4net;
using System.Reflection;
using Microsoft.Extensions.Logging; // Add this using directive
using Microsoft.Extensions.DependencyInjection; // Add this using directive

var builder = WebApplication.CreateBuilder(args);

// Configure Log4Net
builder.Logging.ClearProviders();
builder.Logging.AddLog4Net("log4net.config");


// Add services to the container.
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(); // Register HttpClient
builder.Services.AddSingleton<ICurrencyService, CurrencyService>(); // Register Currency Service for Application LifeTime
builder.Services.AddHostedService<CurrencyRateUpdaterService>(); // Register Background Service

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
