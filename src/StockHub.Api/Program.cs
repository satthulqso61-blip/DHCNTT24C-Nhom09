using Microsoft.OpenApi;
using StockHub.Api.Middleware;
using StockHub.Application;
using StockHub.Application.Common.Interfaces;
using StockHub.Infrastructure;
using StockHub.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. Add API Controllers & JSON options
builder.Services.AddControllers();

// 3. Add CORS for Frontend Vite dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. Configure Swagger UI with JWT Bearer support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StockHub API",
        Version = "v1",
        Description = "StockHub Multi-Warehouse Inventory Management API (.NET 10)"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token directly. Example: 'eyJhbGci...'"
    });

    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });

});

var app = builder.Build();

// 5. Seed default accounts (Admin, WarehouseStaff, Accountant) on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await StockHubDbContextSeeder.SeedAsync(dbContext, passwordHasher);
}

// 6. Global Exception Handling Middleware (RFC 9457 ProblemDetails)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 7. Configure Swagger (Enable in Development and Production for demo)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "StockHub API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
