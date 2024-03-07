using Microsoft.OpenApi.Models;
using Serilog;
using IsoBridge.ISO8583;
using IsoBridge.Infrastructure.Audit;


var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration);
});

// Services
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IsoBridge API", Version = "v1" });
});
builder.Services.AddIso8583(builder.Configuration);
builder.Services.AddIsoBridgeInfrastructure(builder.Configuration);

var app = builder.Build();

// Security hardening (basic for now, will update later)
app.UseHsts();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllers();


app.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow }));

app.Run();