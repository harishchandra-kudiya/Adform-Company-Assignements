using Adfrom_CurrencyConversionDB.Data;
using Adfrom_CurrencyConversionDB.Interfaces;
using Adfrom_CurrencyConversionDB.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using log4net.Config;
using log4net;
using System.Reflection;
using Microsoft.Extensions.Logging; // Add this using directive
using Microsoft.Extensions.DependencyInjection; // Add this using directive

var builder = WebApplication.CreateBuilder(args);

// Configure Log4Net
builder.Logging.ClearProviders();
builder.Logging.AddLog4Net("log4net.config");

// Register DbContext (Scoped by default)
builder.Services.AddDbContext<CurrencyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register HttpClient for CurrencyService
builder.Services.AddHttpClient<CurrencyService>();

// Register CurrencyService as Scoped
builder.Services.AddScoped<ICurrencyService, CurrencyService>();

// Register Background Service
builder.Services.AddHostedService<CurrencyRateUpdaterService>();


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
