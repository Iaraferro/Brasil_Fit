# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Projeto

BrasilFit / NutriLife — API REST de planejamento nutricional em **ASP.NET Core 10 + EF Core 10 + SQL Server**.

## Comandos

```powershell
# Restaurar dependências
dotnet restore

# Rodar a API (migrations e seed aplicados automaticamente na inicialização)
dotnet run --project src/BrasilFit.API

# Build sem rodar
dotnet build

# Criar nova migration (--project = Infrastructure, --startup-project = API)
dotnet ef migrations add <NomeDaMigration> `
  --project src/BrasilFit.Infrastructure `
  --startup-project src/BrasilFit.API `
  --output-dir Data/Migrations

# Aplicar migrations manualmente (opcional — startup já faz isso via MigrateAsync)
dotnet ef database update `
  --project src/BrasilFit.Infrastructure `
  --startup-project src/BrasilFit.API

# Reverter última migration
dotnet ef migrations remove `
  --project src/BrasilFit.Infrastructure `
  --startup-project src/BrasilFit.API
```

Não há projeto de testes ainda. Swagger disponível em `https://localhost:5001/swagger` quando rodando em Development.

## Arquitetura

Solução com 3 projetos — referências fluem em uma única direção: `API → Infrastructure → Domain`.

```
BrasilFit.Domain        — entidades e enums; zero dependências externas
BrasilFit.Infrastructure — AppDbContext, Repository<T>, DataSeeder, ViaCepService, OpenFoodFactsService
BrasilFit.API           — Controllers, DTOs, Services de aplicação, Program.cs
```

**Domain** contém apenas `Entities/` e `Enums/`. Nenhuma referência a EF, ASP.NET ou HTTP.

**Infrastructure** não referencia BrasilFit.API. O `AppDbContext` é registrado com `MigrationsAssembly` apontado para o próprio assembly de Infrastructure — por isso o `--project` do `dotnet ef` sempre aponta para Infrastructure.

**API** hospeda os _Application Services_ (`Services/Auth/`, `Services/Pacientes/`, etc.) — diferente do padrão clássico onde services ficam em camada separada. A escolha é intencional para manter o projeto pequeno.

## Padrões críticos

### Herança de Usuario (TPH)
`Administrador`, `Nutricionista` e `Paciente` herdam de `Usuario` (abstrata). Mapeados em tabela única `Usuarios` com discriminador `Papel` (enum `PapelUsuario`). Para queries em subtipos use `_context.Nutricionistas`, `_context.Pacientes`, etc. — não `_context.Usuarios`.

### Exceções como controle de fluxo de negócio
Controllers **não possuem try/catch**. `GlobalExceptionHandler` (`Middlewares/GlobalExceptionHandler.cs`) intercepta e traduz:
- `InvalidOperationException` → 400 (regra de negócio violada)
- `UnauthorizedAccessException` → 403
- `KeyNotFoundException` → 404
- demais → 500

Services lançam essas exceções diretamente. Não adicione try/catch nos controllers.

### Paginação
Toda listagem retorna `PaginacaoResultadoDto<T>` (`DTOs/Common/`). Controllers aceitam `[FromQuery] PaginacaoQuery` com clamp automático em `TamanhoPagina` (máx. 100). Use `Skip/Take` sobre `IQueryable` — nunca `ToList()` antes de filtrar.

### Repository vs AppDbContext direto
`IRepository<T>` / `Repository<T>` existe para operações CRUD simples. Para queries com `.Include()`, filtros compostos ou paginação, injete `AppDbContext` diretamente no Service — é explicitamente preferido neste projeto.

### HttpClientFactory (APIs externas)
`ViaCepService` e `OpenFoodFactsService` são **typed clients** registrados com `AddHttpClient<TInterface, TImpl>()`. Nunca instancie `HttpClient` manualmente. As BaseAddresses vêm de `appsettings.json → ExternalApis`.

### JSON global
Configurado em `Program.cs` via `AddJsonOptions`:
- `ReferenceHandler.IgnoreCycles` — essencial por causa das navegações circulares do EF
- `JsonStringEnumConverter` — enums viram string no JSON (`"Nutricionista"`, não `2`)
- `DefaultIgnoreCondition = WhenWritingNull` — campos nulos não são enviados

Não sobrescreva essas opções localmente em controllers.

## Configuração

Variáveis sensíveis que precisam ser ajustadas antes de rodar:

| Chave em appsettings.json | Valor padrão | O que mudar |
|---|---|---|
| `ConnectionStrings:Default` | `Server=localhost;...` | Ajustar para o SQL Server local |
| `Jwt:SecretKey` | placeholder 32+ chars | Trocar por valor secreto real |
| `Cors:OrigensPermitidas` | localhost 5173/3000/4200 | Adicionar URL do front em produção |
| `Seed:Admin:Senha` | `Admin@123` | Opcional; só afeta o primeiro seed |

`Cors:OrigensPermitidas` vazio → política permissiva (fallback de dev). Com origens preenchidas → `AllowCredentials()` é ativado automaticamente.

O `DataSeeder` roda no startup via `MigrateAsync` + seed de 1 Administrador e 10 alimentos. É idempotente — verifica existência antes de inserir.
