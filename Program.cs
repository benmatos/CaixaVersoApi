using CaixaVersoApi.Converters;
using CaixaVersoApi.Data;
using CaixaVersoApi.Filters;
using CaixaVersoApi.Middlewares;
using CaixaVersoApi.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure JSON serialization as per requirements:
// - Snake_case format
// - Ignore nulls
// - Custom date format
builder.Services.AddControllers(options =>
{
    // Global filter
    options.Filters.Add<StandardizedResponseFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
    options.JsonSerializerOptions.Converters.Add(new CustomNullableDateTimeConverter());
});

// Configure CORS
var corsOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Dependency Injection
var persistenceType = builder.Configuration["PersistenceType"] ?? "Memory";

if (persistenceType.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<CaixaVersoDbContext>(options =>
        options.UseSqlServer(connectionString));
    builder.Services.AddScoped<IUsuarioRepository, UsuarioSqlRepository>();
}
else
{
    builder.Services.AddSingleton<IUsuarioRepository, UsuarioRepository>();
}

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => 
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

// Apply pending migrations when using SQL Server
if (persistenceType.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CaixaVersoDbContext>();
    db.Database.Migrate();
}

// Custom CORS (must be before UseHttpsRedirection and other middleware)
app.UseCors("FrontendPolicy");

app.UseHttpsRedirection();

// Custom Middleware
app.UseMiddleware<ResponseTimeMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
