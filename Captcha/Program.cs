using Captcha.Configuration;
using Captcha.ExceptionHandling;
using Captcha.Interfaces;
using Captcha.Repository;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(
        path: "Logs/app-log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,   // optional: keep 30 days
        rollOnFileSizeLimit: false,
        shared: true
    )
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<ICaptchaStore, CaptchaStore>();
builder.Services.AddScoped<ICaptchaTextGeneratorService, CaptchaTextGeneratorService>();
builder.Services.AddScoped<ICaptchaImageGeneratorService, CaptchaImageGeneratorService>();
builder.Services.AddScoped<IValidateCaptchaService, ValidateCaptchaService>();
builder.Services.AddScoped<ICaptchaService, CaptchaService>();
builder.Services.AddSingleton<ICaptchaFontLoader, CaptchaFontLoader>();
builder.Services.AddSingleton<ICaptchaFontProvider, CaptchaFontProvider>();
builder.Services.AddSingleton<IRandomProvider, CryptoRandomProvider>();
builder.Services.AddSingleton<ICaptchaHashService, CaptchaHashService>();
builder.Services.Configure<CaptchaOptions>(builder.Configuration.GetSection("Captcha"));

var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
