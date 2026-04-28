# Documentação Técnica — CaixaVerso API

## Visão Geral

API REST desenvolvida em **.NET 8** para gerenciamento de usuários. Permite cadastrar, listar, buscar, atualizar e desativar usuários, com proteção de senha, respostas padronizadas e suporte a dois modos de persistência.

---

## Instruções de Execução

Execute:

```bash
dotnet run
```

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB (opcional — apenas para o modo SQL Server)

### Modo memória (sem banco de dados)

Abra `appsettings.json` e altere:

```json
"PersistenceType": "Memory"
```

### Modo SQL Server

Mantenha `"PersistenceType": "SqlServer"` em `appsettings.json` e certifique-se de que a string de conexão aponta para uma instância válida:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CaixaVerso;Trusted_Connection=True;TrustServerCertificate=True;"
}

As migrations são aplicadas automaticamente na inicialização.

### Swagger UI

Após iniciar, acesse `http://localhost:{porta}/` no navegador para abrir o Swagger UI com todos os endpoints documentados.

---

## Arquitetura em Camadas

CaixaVersoApi/
├── Controllers/      → Recebe as requisições HTTP e devolve respostas
├── DTOs/             → Contratos de entrada e saída (nunca expõe SenhaHash)
├── Models/           → Entidade de domínio (Usuario)
├── Repositories/     → Acesso a dados (interface + implementações)
├── Data/             → DbContext e factory do Entity Framework Core
├── Filters/          → Filtro de padronização de resposta
├── Middlewares/      → Middleware de log de tempo de resposta
├── Converters/       → Conversor de datas para o formato dd/MM/yyyy HH:mm:ss
└── Program.cs        → Composição root da aplicação
```

---

## Endpoints

Base path: `/api/v1/usuarios`

| Método | Rota                      | Descrição                        | Status de sucesso |
|--------|---------------------------|----------------------------------|-------------------|
| POST   | `/api/v1/usuarios`        | Cadastra um novo usuário         | 201 Created       |
| GET    | `/api/v1/usuarios`        | Lista todos os usuários          | 200 OK            |
| GET    | `/api/v1/usuarios/{id}`   | Busca um usuário pelo GUID       | 200 OK            |
| PUT    | `/api/v1/usuarios/{id}`   | Atualiza nome e cargo            | 200 OK            |
| DELETE | `/api/v1/usuarios/{id}`   | Desativa o usuário (soft delete) | 200 OK            |

### Exemplo — Criar usuário

**Request:**
```json
POST /api/v1/usuarios
{
  "nome": "João Silva",
  "email": "joao@exemplo.com",
  "senha": "senha123",
  "cargo": "Analista"
}
```

**Response (201):**
```json
{
  "dados_resposta": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nome": "João Silva",
    "email": "joao@exemplo.com",
    "ativo": true,
    "criado_em": "28/04/2026 10:30:00",
    "cargo": "Analista"
  },
  "timestamp_resposta": "28/04/2026 10:30:00",
  "tempo_da_resposta": "45 ms"
}
```

---

## Decisões Técnicas

### Proteção de senha

A senha é convertida com **BCrypt** (`BCrypt.Net-Next`) antes de ser salva. O hash inclui salt aleatório, tornando cada hash único mesmo para senhas iguais. O campo `SenhaHash` nunca é exposto nas respostas da API.

### Middleware — `ResponseTimeMiddleware`

Mede o tempo total da requisição e registra no log da aplicação:
Request GET /api/v1/usuarios took 12 ms

### Filtro — `StandardizedResponseFilter`

Filtro de ação global (`IAsyncActionFilter`) que envolve qualquer `ObjectResult` no envelope padrão:

```json
{
  "dados_resposta": { ... },
  "timestamp_resposta": "dd/MM/yyyy HH:mm:ss",
  "tempo_da_resposta": "xxx ms"
}
```

### Serialização JSON

Configurada globalmente em `Program.cs`:

- **snake_case** para todos os campos (ex.: `criado_em`, `senha_hash`)
- **Nulos ignorados**: campos `null` não aparecem na resposta
- **Datas** no formato `dd/MM/yyyy HH:mm:ss` via `CustomDateTimeConverter`

### CORS

Configurado via `appsettings.json` (seção `CorsSettings:AllowedOrigins`). Por padrão, apenas o domínio do frontend é autorizado:

```
https://brunotrbr.github.io
```

Qualquer método e cabeçalho são permitidos para esse domínio. Alterações de domínio não exigem recompilação.

### Roteamento customizado

O controller usa versionamento na rota: `api/v1/[controller]`. O segmento `{id:guid}` aplica restrição de tipo, rejeitando automaticamente IDs em formato inválido.

### Repositório e inversão de dependência

A interface `IUsuarioRepository` desacopla o controller da fonte de dados. O `Program.cs` decide qual implementação injetar com base na configuração:

- `UsuarioRepository` — dicionário em memória (`Singleton`)
- `UsuarioSqlRepository` — Entity Framework Core + SQL Server (`Scoped`)

### Desativação de usuário (soft delete)

O endpoint `DELETE` não remove o registro do banco. Ele apenas define `Ativo = false`, preservando o histórico e a integridade referencial.

---

## Dependências (NuGet)

| Pacote                                   | Versão | Uso                              |
|------------------------------------------|--------|----------------------------------|
| `BCrypt.Net-Next`                        | 4.1.0  | Hash seguro de senha             |
| `Microsoft.EntityFrameworkCore.SqlServer`| 8.0.0  | ORM para SQL Server              |
| `Microsoft.EntityFrameworkCore.Tools`    | 8.0.0  | Migrations via CLI               |
| `Swashbuckle.AspNetCore`                 | 6.4.0  | Documentação Swagger             |

---

## Diagrama de Camadas

```
[ Cliente HTTP ]
       │
       ▼
[ Middleware: ResponseTimeMiddleware ]
       │
       ▼
[ Controller: UsuariosController ]
       │  (via IActionFilter)
       ▼
[ Filtro: StandardizedResponseFilter ]
       │
       ▼
[ Repository: IUsuarioRepository ]
       │
  ┌────┴────┐
  ▼         ▼
[Memória] [SQL Server via EF Core]
```
