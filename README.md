# BrasilFit / NutriLife — API

API REST em **ASP.NET Core 10** + **EF Core 10** + **SQL Server** para o sistema de planejamento nutricional **BrasilFit (NutriLife)**.

---

## Estrutura

```
Brasil_Fit/
├── BrasilFit.sln
└── src/
    ├── BrasilFit.Domain/            ← Entidades e Enums (sem dependências)
    │   ├── Entities/                ← 16 entidades de domínio
    │   └── Enums/
    ├── BrasilFit.Infrastructure/    ← EF Core, DbContext, Repos, APIs externas
    │   ├── Data/
    │   │   ├── AppDbContext.cs
    │   │   └── Seeders/DataSeeder.cs
    │   ├── ExternalServices/
    │   │   ├── ViaCep/
    │   │   └── OpenFoodFacts/
    │   └── Repositories/
    └── BrasilFit.API/               ← Controllers, DTOs, Services, Program.cs
        ├── Controllers/
        ├── DTOs/
        ├── Services/
        ├── appsettings.json
        └── Program.cs
```

### Por que essa divisão em 3 projetos?
- **Domain** não conhece EF, ASP.NET ou HTTP. Pode ser referenciado por qualquer camada e testado isoladamente.
- **Infrastructure** concentra tudo que toca o "mundo externo": banco e APIs.
- **API** é só a camada de entrada (Controllers + DTOs) + serviços de aplicação que orquestram regras.

---

## Pré-requisitos

| Ferramenta              | Versão           |
|-------------------------|------------------|
| .NET SDK                | 10.0+            |
| SQL Server              | 2019+ / Express  |
| EF Core Tools           | `dotnet tool install --global dotnet-ef`  |

---

## Como rodar

### 1. Restaurar pacotes
```powershell
dotnet restore
```

### 2. Ajustar a connection string em `src/BrasilFit.API/appsettings.json`
```json
"ConnectionStrings": {
  "Default": "Server=localhost;Database=BrasilFit;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 3. Criar a primeira migration
> O `--project` aponta para o assembly que contém o `DbContext` e o `--startup-project` para a API (onde está a connection string).

```powershell
dotnet ef migrations add InitialCreate `
  --project src/BrasilFit.Infrastructure/BrasilFit.Infrastructure.csproj `
  --startup-project src/BrasilFit.API/BrasilFit.API.csproj `
  --output-dir Data/Migrations
```

### 4. Aplicar no banco
```powershell
dotnet ef database update `
  --project src/BrasilFit.Infrastructure/BrasilFit.Infrastructure.csproj `
  --startup-project src/BrasilFit.API/BrasilFit.API.csproj
```
> *Obs:* o `Program.cs` já chama `Database.MigrateAsync()` na inicialização, então rodar `dotnet ef database update` é opcional — útil quando você quer aplicar sem subir a API.

### 5. Rodar a API
```powershell
dotnet run --project src/BrasilFit.API
```

Acesse o Swagger em `https://localhost:5001/swagger` (ou a porta que o Kestrel imprimir no console).

---

## Credenciais padrão (seed)
| Email                   | Senha     | Papel         |
|-------------------------|-----------|---------------|
| `admin@brasilfit.local` | `Admin@123` | Administrador |

> Customizáveis em `appsettings.json` na seção `Seed:Admin`.

---

## Endpoints

### Públicos
| Método | Rota                                     | Descrição                                |
|--------|------------------------------------------|------------------------------------------|
| POST   | `/api/auth/login`                        | Login → retorna JWT                      |
| GET    | `/api/enderecos/cep/{cep}`               | Consulta CEP no ViaCEP                   |
| GET    | `/api/alimentos/openfoodfacts/codigo/{codigoBarras}` | Busca alimento por código de barras (OFF) |
| GET    | `/api/alimentos/openfoodfacts/buscar?termo=...`      | Busca alimentos por nome (OFF)            |

### Autenticados (`[Authorize(Roles = "Nutricionista")]`)
| Método | Rota                          | Descrição                              |
|--------|-------------------------------|----------------------------------------|
| POST   | `/api/pacientes`              | Cadastrar paciente                     |
| GET    | `/api/pacientes`              | Listar pacientes do nutricionista logado |
| POST   | `/api/planos-alimentares`     | Criar plano alimentar                  |
| POST   | `/api/avaliacoes`             | Registrar avaliação antropométrica     |

---

## Domínio (16 entidades)
`Usuario` (abstrata) → `Administrador`, `Nutricionista`, `Paciente` (TPH), `Endereco`, `PlanoAlimentar`, `Refeicao`, `ItemRefeicao`, `Alimento`, `AvaliacaoAntropometrica`, `MedidaCorporal`, `ComposicaoCorporal`, `Meta`, `ProgressoMeta`, `Notificacao`, `LogAuditoria`.

Relacionamentos:
- **Herança TPH**: `Usuarios` (tabela única, discriminador `Papel`).
- **1:1** — `Paciente`/`Endereco`, `Avaliacao`/`MedidaCorporal`, `Avaliacao`/`ComposicaoCorporal`.
- **1:N** — `Nutricionista`/`Pacientes`, `Paciente`/`Planos`/`Avaliacoes`/`Metas`, `PlanoAlimentar`/`Refeicoes`, `Meta`/`Progressos`, `Usuario`/`Notificacoes`+`Logs`.
- **N:N com payload** — `Refeicao` ↔ `Alimento` via `ItemRefeicao` (Quantidade, Unidade).

---

## Integrações externas (HttpClientFactory)

| Serviço          | BaseUrl                          | Cliente tipado            |
|------------------|----------------------------------|---------------------------|
| ViaCEP           | `https://viacep.com.br/`         | `ViaCepService`           |
| OpenFoodFacts    | `https://world.openfoodfacts.org/` | `OpenFoodFactsService`  |

Ambos são registrados via `AddHttpClient<TInterface, TImplementacao>()` em `Program.cs`, o que garante:
- Pool de conexões reutilizável (resolve o `SocketException` clássico de instanciar `HttpClient` manualmente).
- Timeout configurado por cliente.
- Pronto para Polly (retry/circuit-breaker) no futuro.
