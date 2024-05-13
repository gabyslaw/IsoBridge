using Microsoft.OpenApi.Models;
using Serilog;
using IsoBridge.ISO8583;
using IsoBridge.Infrastructure.Audit;
using IsoBridge.Web.Services;
using IsoBridge.Adapters;


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
builder.Services.AddIsoBridgeAdapters(builder.Configuration);

builder.Services.AddScoped<AuditLoggingService>();
builder.Services.AddScoped<ForwardingService>();


var app = builder.Build();

// Security hardening (basic for now, will update later)
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSerilogRequestLogging();

// Swagger (dev only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// MVC controller routes
app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{controller=AdminAudit}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow }));

app.Run();