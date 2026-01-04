using Captcha.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
