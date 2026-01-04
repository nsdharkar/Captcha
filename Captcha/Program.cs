using Captcha.Repository;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    //.MinimumLevel.Override("System", LogEventLevel.Warning)
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

builder.Services.AddSession();

builder.Services.AddScoped<ICaptchaTextGeneratorService, CaptchaTextGeneratorService>();
builder.Services.AddScoped<ICaptchaImageGeneratorService, CaptchaImageGeneratorService>();
builder.Services.AddLogging();
//builder.Services.AddHttpContextAccessor();

var app = builder.Build();

CaptchaFontManager.Initialize(app.Services.GetRequiredService<ILogger<CaptchaFontManager>>());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSession();

app.MapControllers();

app.Run();
