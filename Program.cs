using CaixaVersoApi.Converters;
using CaixaVersoApi.Data;
using CaixaVersoApi.Filters;
using CaixaVersoApi.Middlewares;
using CaixaVersoApi.Repositories;
using CaixaVersoApi.Services;
using CaixaVersoApi.Models;
using Microsoft.EntityFrameworkCore;        
// Ponto de entrada da aplicação.
// O WebApplication.CreateBuilder configura o host, lê o appsettings.json
// e prepara o contêiner de injeção de dependência.
// Configura automaticamente os componentes necessários para o sistema operar.
var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────
// CONFIGURAÇÃO DOS CONTROLLERS E SERIALIZAÇÃO JSON
// ─────────────────────────────────────────────

// Registra os controllers e aplica configurações globais de serialização:
// - Snake_case: propriedades retornadas como "nome_completo" ao invés de "NomeCompleto"
// - Ignora nulos: campos null não aparecem na resposta JSON
// - Formato de data personalizado via CustomDateTimeConverter
builder.Services.AddControllers(options =>
{
    // Filtro global: padroniza o formato de todas as respostas da API
    options.Filters.Add<StandardizedResponseFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
    options.JsonSerializerOptions.Converters.Add(new CustomNullableDateTimeConverter());
});

// ─────────────────────────────────────────────
// CONFIGURAÇÃO DE CORS
// ─────────────────────────────────────────────

// Lê as origens permitidas do appsettings.json (seção CorsSettings:AllowedOrigins).
// CORS define quais domínios front-end podem consumir esta API.
var corsOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(corsOrigins)  // Apenas origens configuradas são permitidas
              .AllowAnyMethod()          // GET, POST, PUT, DELETE, etc.
              .AllowAnyHeader();         // Authorization, Content-Type, etc.
    });
});

// ─────────────────────────────────────────────
// INJEÇÃO DE DEPENDÊNCIA — REPOSITÓRIO DE USUÁRIOS
// ─────────────────────────────────────────────

// A persistência é configurada pelo appsettings.json (chave "PersistenceType").
// "SqlServer" → usa banco de dados real com Entity Framework Core.
// Qualquer outro valor → usa repositório em memória (ideal para testes e aulas).
var persistenceType = builder.Configuration["PersistenceType"] ?? "Memory";

if (persistenceType.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // Registra o DbContext com a string de conexão do appsettings.json
    builder.Services.AddDbContext<CaixaVersoDbContext>(options =>
        options.UseSqlServer(connectionString));

    // Scoped: uma instância por requisição HTTP (recomendado para repositórios com DbContext)
    builder.Services.AddScoped<IUsuarioRepository, UsuarioSqlRepository>();
}
else
{
    // Singleton: uma única instância compartilhada durante toda a vida da aplicação
    // Adequado para o repositório em memória (dicionário estático)
    builder.Services.AddSingleton<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddSingleton<UsuarioService>();
    builder.Services.AddSingleton<Dictionary<string, Usuario>>();
}

// CriptografiaService é necessário em ambos os modos (Memory e SqlServer)
builder.Services.AddSingleton<CriptografiaService>();

// ─────────────────────────────────────────────
// SWAGGER — DOCUMENTAÇÃO AUTOMÁTICA DA API
// ─────────────────────────────────────────────

// Gera o arquivo swagger.json a partir dos controllers e anotações XML
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Constrói a aplicação com todas as configurações acima
var app = builder.Build();

// ─────────────────────────────────────────────
// PIPELINE DE REQUISIÇÃO HTTP (MIDDLEWARES)
// ─────────────────────────────────────────────

// Habilita o Swagger apenas em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => 
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty; // Swagger abre na raiz: http://localhost:{porta}/
    });
}

// Executa as migrations pendentes automaticamente ao iniciar com SQL Server
if (persistenceType.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CaixaVersoDbContext>();
    db.Database.Migrate(); // Cria/atualiza as tabelas no banco de dados
}

// CORS deve ser registrado antes de UseHttpsRedirection e outros middlewares
app.UseCors("FrontendPolicy");

// Redireciona automaticamente requisições HTTP para HTTPS
app.UseHttpsRedirection();

// Middleware personalizado: mede e adiciona o tempo de resposta no header HTTP
app.UseMiddleware<ResponseTimeMiddleware>();

// Habilita verificação de autorização (JWT, Policies, etc.)
app.UseAuthorization();

// Mapeia as rotas definidas nos Controllers via atributos [HttpGet], [HttpPost], etc.
app.MapControllers();

// Inicia o servidor e começa a escutar requisições
app.Run();
