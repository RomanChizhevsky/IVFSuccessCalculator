using FluentValidation;
using IVFSuccessCalculator.BusinessLogic;
using IVFSuccessCalculator.Data;
using IVFSuccessCalculator.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder
    .Services
    .AddTransient<ISuccessRateCalculator, SuccessRateCalculator>()
    .AddTransient<IValidator<SuccessRateCalculationParameters>, SuccessRateRequestValidator>()
    .Configure<List<SuccessRateFormulaParameters>>(s => s.AddRange(SuccessRateFormulaRepository.FetchParameters()))
    .AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseCors(c => c
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
