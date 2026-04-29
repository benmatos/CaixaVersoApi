# Documentação Técnica — CaixaVerso API

## Visão Geral

API REST desenvolvida em **.NET 8** para gerenciamento de usuários. Permite cadastrar, listar, buscar, atualizar e desativar usuários, com proteção de senha, criptografia AES-256 da data de nascimento, respostas padronizadas e suporte a dois modos de persistência.

---

## Instruções de Execução

Execute:

```bash
dotnet run
```

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB (opcional — apenas para o modo SQL Server)

### Modo memória (sem banco de dados - padrão)

```json
"PersistenceType": "Memory"
```

### Modo SQL Server

Abra `appsettings.json` e altere:
"PersistenceType": "SqlServer" e certifique-se de que a string de conexão aponta para uma instância válida:

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
├── Converters/       → Conversor de datas (entrada flexível, saída dd/MM/yyyy HH:mm:ss)
├── Services/         → CriptografiaService (AES-256) e UsuarioService
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
  "cargo": "Analista",
  "data_nascimento": "15/06/1990"
}
```

> O campo `data_nascimento` aceita os formatos: `dd/MM/yyyy`, `dd/MM/yyyy HH:mm:ss`, `yyyy-MM-dd` e ISO 8601 (`2026-04-29T02:05:28.868Z`).

**Response (201):**
```json
{
  "dados_resposta": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nome": "João Silva",
    "email": "joao@exemplo.com",
    "ativo": true,
    "criado_em": "28/04/2026 10:30:00",
    "cargo": "Analista",
    "data_nascimento": "15/06/1990 00:00:00"
  },
  "timestamp_resposta": "28/04/2026 10:30:00",
  "tempo_da_resposta": "45 ms"
}
```

---

## Decisões Técnicas

### Proteção de senha

A senha é convertida com **BCrypt** (`BCrypt.Net-Next`) antes de ser salva. O hash inclui salt aleatório, tornando cada hash único mesmo para senhas iguais. O campo `SenhaHash` nunca é exposto nas respostas da API.

### Criptografia da data de nascimento

A data de nascimento é criptografada com **AES-256** antes de ser persistida, por meio do `CriptografiaService`. A chave e o IV (IV introduz aleatoriedade no processo de cifragem) são lidos do `appsettings.json` (seção `Criptografia`) como strings Base64:

```json
"Criptografia": {
  "KeyBase64": "<chave AES-256 de 32 bytes em Base64>",
  "IvBase64": "<IV de 16 bytes em Base64>"
}
```

> **Segurança:** em produção, mova esses valores para variáveis de ambiente ou `dotnet user-secrets`. Nunca commite chaves reais no repositório.

Para gerar novos valores:
```powershell
$aes = [System.Security.Cryptography.Aes]::Create()
$aes.GenerateKey(); $aes.GenerateIV()
Write-Host "KeyBase64: $([Convert]::ToBase64String($aes.Key))"
Write-Host "IvBase64:  $([Convert]::ToBase64String($aes.IV))"
```

Na resposta da API, a data é **descriptografada** e retornada no formato `dd/MM/yyyy HH:mm:ss`.

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

- **snake_case** para todos os campos (ex.: `criado_em`, `data_nascimento`)
- **Nulos ignorados**: campos `null` não aparecem na resposta
- **Datas na entrada**: aceita `dd/MM/yyyy`, `dd/MM/yyyy HH:mm:ss`, `yyyy-MM-dd`, `yyyy-MM-ddTHH:mm:ss` e ISO 8601 completo (com milissegundos e `Z`)
- **Datas na saída**: sempre `dd/MM/yyyy HH:mm:ss` via `CustomDateTimeConverter`

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

## Descrição das Classes

### Controllers

#### `UsuariosController`
Ponto de entrada HTTP da aplicação. Recebe as requisições REST, delega a lógica ao repositório e ao serviço de criptografia, e devolve as respostas formatadas. Cada método corresponde a um endpoint (`POST`, `GET`, `PUT`, `DELETE`). Usa injeção de dependência para receber `IUsuarioRepository` e `CriptografiaService`, sem acoplamento com implementações concretas.

---

### DTOs

#### `UsuarioDto`
Contrato de **saída** da API. Representa os dados do usuário que são retornados ao cliente. Nunca expõe o campo `SenhaHash`, garantindo que a senha nunca vaze pela resposta.

#### `CriarUsuarioDto`
Contrato de **entrada** para criação de um novo usuário. Contém validações via Data Annotations (`[Required]`, `[EmailAddress]`, `[MinLength]`) que são verificadas automaticamente pelo pipeline do ASP.NET Core antes de o controller ser executado.

#### `AtualizarUsuarioDto`
Contrato de **entrada** para atualização parcial de um usuário. Restringe os campos que o cliente pode modificar (nome, cargo e data de nascimento), impedindo alterações de campos sensíveis como e-mail ou senha via esse endpoint.

---

### Models

#### `Usuario`
Entidade de domínio que representa um usuário no sistema. É a classe mapeada pelo Entity Framework Core para a tabela do banco de dados. Armazena a senha como hash (`SenhaHash`) e a data de nascimento de forma criptografada (`DataNascimentoCriptografada`), nunca em texto puro.

---

### Repositories

#### `IUsuarioRepository`
Interface que define o contrato de acesso a dados. Permite que o restante do sistema (controller, serviços) trabalhe com qualquer fonte de dados sem depender de uma implementação específica — princípio da inversão de dependência (DIP). Define as operações: `CriarAsync`, `ListarAsync`, `BuscarPorIdAsync`, `BuscarPorEmailAsync` e `AtualizarAsync`.

#### `UsuarioRepository`
Implementação em memória de `IUsuarioRepository`. Usa um `Dictionary<Guid, Usuario>` como armazenamento. Ideal para desenvolvimento, testes e demonstrações sem necessidade de banco de dados. Registrado como `Singleton` no contêiner de DI.

#### `UsuarioSqlRepository`
Implementação de `IUsuarioRepository` que persiste os dados no SQL Server via Entity Framework Core. Usa o `CaixaVersoDbContext` para executar as operações no banco. Registrado como `Scoped` no contêiner de DI para seguir o ciclo de vida correto do `DbContext`.

---

### Data

#### `CaixaVersoDbContext`
Contexto do Entity Framework Core. Define o `DbSet<Usuario>` e configura o mapeamento da entidade no método `OnModelCreating` (chave primária, tamanhos máximos de campos, índice único para e-mail). É a ponte entre os objetos C# e as tabelas do banco de dados.

#### `CaixaVersoDbContextFactory`
Fábrica de design-time usada exclusivamente pelas ferramentas do EF Core (`dotnet ef migrations add`, `dotnet ef database update`). Permite que os comandos de migração funcionem sem precisar iniciar a aplicação completa.

---

### Filters

#### `StandardizedResponseFilter`
Filtro de ação global (`IAsyncActionFilter`) que intercepta qualquer `ObjectResult` retornado pelos controllers e o envolve no envelope padronizado com `dados_resposta`, `timestamp_resposta` e `tempo_da_resposta`. Garante que **todas** as respostas sigam o mesmo formato, sem que cada controller precise fazer isso manualmente.

---

### Middlewares

#### `ResponseTimeMiddleware`
Middleware customizado que mede o tempo total de cada requisição HTTP usando um `Stopwatch`. Ao final da requisição, registra no log da aplicação a informação de tempo no formato `Request {Método} {Rota} took {N} ms`. É executado antes de qualquer controller, medindo o tempo de toda a pipeline.

---

### Converters

#### `CustomDateTimeConverter`
Conversor de datas para o serializador `System.Text.Json`. Na **leitura** (entrada), aceita múltiplos formatos (`dd/MM/yyyy`, `dd/MM/yyyy HH:mm:ss`, `yyyy-MM-dd`, ISO 8601), tornando a API tolerante a diferentes clientes. Na **escrita** (saída), normaliza todas as datas para `dd/MM/yyyy HH:mm:ss`, garantindo consistência nas respostas.

#### `CustomNullableDateTimeConverter`
Variante de `CustomDateTimeConverter` para campos `DateTime?` (nullable). Aplica a mesma lógica de leitura e escrita, mas aceita valores nulos sem lançar exceção.

---

### Services

#### `CriptografiaService`
Serviço responsável por criptografar e descriptografar dados sensíveis usando **AES-256**. A chave (`KeyBase64`) e o vetor de inicialização (`IvBase64`) são lidos do `appsettings.json` via `IConfiguration`. Usado pelo controller para proteger a data de nascimento antes de persistir e para restaurá-la ao retornar os dados.

#### `UsuarioService`
Serviço auxiliar que encapsula a lógica de criação e busca de usuários com suporte à criptografia. Combina o dicionário de dados em memória com o `CriptografiaService` para criar e mapear entidades com data de nascimento protegida.

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
       │  usa CriptografiaService (AES-256)
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
