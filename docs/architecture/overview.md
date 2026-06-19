# Visão Geral da Arquitetura

## Estilo Arquitetural

O ParkingSystem adota **Modular Monolith** — uma aplicação única implantável, mas com módulos internos com fronteiras bem definidas, cada um com suas próprias camadas e responsabilidades. Essa abordagem combina a simplicidade operacional de um monolito com o isolamento e a organização de uma arquitetura de microsserviços, sem a complexidade de rede distribuída.

Dentro de cada módulo, são aplicados os princípios de **Domain-Driven Design (DDD)** e **Command Query Responsibility Segregation (CQRS)**.

---

## Módulos

### `parking` — Domínio Principal

Responsável por toda a lógica de negócio do estacionamento: vagas, movimentações, tarifas e reservas.

### `identity` — Autenticação e Autorização

Gerencia usuários, senhas (PBKDF2 via `PasswordHasher<T>`) e roles. Não usa ASP.NET Core Identity completo — apenas o `PasswordHasher` para hash de senhas. JWT é gerado na camada WebApi.

---

## Camadas por Módulo

```
┌─────────────────────────────────────────┐
│               WebApi (BFF)              │  ← Endpoints HTTP, geração de JWT
├────────────────────────┬────────────────┤
│      Application       │   Application  │  ← Commands, Queries, DTOs, Services
├────────────────────────┼────────────────┤
│        Domain          │     Domain     │  ← Entidades, Enums, Interfaces
├────────────────────────┼────────────────┤
│    Infra.Data.EF       │  Infra.Data.EF │  ← DbContext, Repositories, Migrations
└────────────────────────┴────────────────┘
         parking                identity
```

### Domain

- Entidades com lógica de negócio encapsulada (métodos de estado)
- Enums de domínio
- Interfaces de repositório (`IVagaRepository`, `IMovimentacaoRepository`, etc.)
- **Zero dependências externas** — C# puro

### Application

- **Commands** (escrita): `ICommandHandler<TCommand, ValidationResult>`, handlers são `internal`
- **Queries** (leitura): Dapper direto no banco, retornam DTOs com snake_case
- **DTOs**: `[JsonPropertyName]` para serialização snake_case
- **Services de domínio**: `TarifaService`, lógica de cálculo que depende de mais de uma entidade
- Validators: `ValidatorBase<T>` customizado (sem FluentValidation)
- Depende de Domain e Shared.Core

### Infra.Data.EF

- `DbContext` com configurações EF Core (`IEntityTypeConfiguration<T>`)
- Global query filter de soft delete em todas as entidades `ITrackableEntity`
- Repositórios implementando as interfaces do Domain
- Migrations EF Core
- `IDesignTimeDbContextFactory<T>` para suporte ao CLI `dotnet ef`

---

## Shared

### `Shared.Core`

Biblioteca de abstrações usada por todos os módulos:

| Tipo | Descrição |
|---|---|
| `Entity<TId>` | Base com `Id` e `DomainEvents` |
| `TrackableEntity` | Adiciona `CreatedAt`, `UpdatedAt`, `IsDeleted` (soft delete) |
| `IRepository<T, TId>` | Contrato base de repositório |
| `IUnitOfWork` | Abstração de commit — `Commit(): Task<bool>` |
| `Command` / `CommandHandler<T>` | Base para CQRS de escrita |
| `ValidationResult` | Resultado de operações com lista de erros |
| `PagedResult<T>` | Wrapper de paginação com `TotalItems`, `TotalPages`, `HasNextPage` |
| `DomainValidation` | Helpers estáticos de validação de entidade |
| `EntityValidationException` | Exceção lançada em invariantes violadas |

### `Shared.IoC`

`NativeInjectorBootStrapper.RegisterServices(IServiceCollection)` — registra todos os serviços dos módulos (repositories, queries, command handlers) em um único ponto de entrada, chamado em `Program.cs`.

---

## CQRS

### Escrita (Commands)

```
HTTP Request
    → Endpoint (WebApi)
        → ICommandHandler<TCommand, ValidationResult>
            → Validação do command (IsValid())
            → Regras de negócio (repositórios, entidades)
            → PersistData(repo.UnitOfWork)  ← único Commit por handler
                → ValidationResult com Data (ID/objeto criado)
```

- Handlers são `internal` — acessíveis via `InternalsVisibleTo` pelo WebApi, Shared.IoC e Tests
- Um único `Commit()` por handler (sem commits parciais)
- `result.Data` contém o objeto retornado em caso de sucesso

### Leitura (Queries)

```
HTTP Request
    → Endpoint (WebApi)
        → IXxxQueries (interface)
            → XxxQueries (implementação Dapper)
                → SQL com is_deleted = false manual
                    → DTO com snake_case
```

- Dapper para leituras complexas — sem overhead do EF Core
- Global query filter do EF **não** se aplica ao Dapper — `is_deleted = false` explícito em todos os SQLs
- Paginação via `COUNT(*) + LIMIT/OFFSET` com constantes SQL compartilhadas entre a query de contagem e a de dados

---

## Persistência

### Dois DbContexts

| DbContext | Módulo | Tabelas |
|---|---|---|
| `IdentityDbContext` | Identity | `usuarios` |
| `ParkingDbContext` | Parking | `vagas`, `movimentacoes`, `tarifas`, `reservas` |

Ambos compartilham a mesma connection string. A separação existe para manter o isolamento entre módulos.

### Soft Delete

Todas as entidades que herdam de `TrackableEntity` possuem `IsDeleted`. O EF Core aplica um global query filter (`WHERE is_deleted = false`) automaticamente. Dapper não tem esse filtro — deve ser adicionado manualmente em cada query.

### Migrations

Cada DbContext possui suas próprias migrations independentes:

```bash
# Identity
dotnet ef migrations add <Nome> --project src/modules/identity/ParkingSystem.Module.Identity.Infra.Data.EF --startup-project src/services/workers/MigrateWorker --context IdentityDbContext

# Parking
dotnet ef migrations add <Nome> --project src/modules/parking/ParkingSystem.Module.Parking.Infra.Data.EF --startup-project src/services/workers/MigrateWorker --context ParkingDbContext
```

---

## WebUI

SPA Vue 3 localizada em `src/services/webui/`. Consome a WebAPI via Axios com JWT no header `Authorization: Bearer`.

| Camada | Tecnologia |
|---|---|
| Framework | Vue 3.5 + Composition API + `<script setup>` + TypeScript |
| Build | Vite 6 |
| UI Components | PrimeVue 4.2.5 (tema Aura customizado, primary `#003c90`) |
| Estado global | Pinia 2 (`vagaStore`, `authStore`) |
| Roteamento | Vue Router 4 (guarda de autenticação via `beforeEach`) |
| HTTP | Axios (interceptor injeta JWT; redireciona para `/login` em 401) |
| Gráficos | Chart.js 4 via componente `Chart` do PrimeVue |

**Views:**

| View | Responsabilidade |
|---|---|
| `DashboardView` | KPIs (ocupação, receita, reservas) + gráficos de tendência |
| `VagasView` | Mapa de vagas com cards filtráveis, modal nova vaga (Admin) |
| `FluxoView` | Entrada rápida + fechamento de ticket em split layout |
| `RelatoriosView` | Gráfico financeiro filtrado por período + KPIs |
| `ReservasView` | Tabela de reservas + form inline nova reserva + KPIs do dia |
| `ConfiguracoesView` | Sidebar Perfil/Usuários/Notificações/Backup + settings card |
| `MovimentacoesView` | Histórico de todas as movimentações |
| `TarifasView` | CRUD de tarifas por tipo de vaga |

---

## Workers

| Worker | Tipo | Responsabilidade |
|---|---|---|
| `MigrateWorker` | Console App (Exe) | Executa `MigrateAsync()` em ambos os DbContexts na inicialização da infra |
| `DataSeedWorker` | Console App (Exe) | Seed idempotente: admin user, 4 tarifas, 9 vagas |
| `ReservaExpiracaoWorker` | `BackgroundService` | Expira reservas vencidas a cada N minutos; usa `IServiceScopeFactory` para resolver DbContext |

Os workers Console usam `IServiceScopeFactory` ou instanciam DbContexts diretamente via `DbContextOptionsBuilder` — sem host completo.

---

## Segurança

- JWT **HMAC-SHA256**, validação de issuer, audience e tempo de vida
- Senhas armazenadas como hash PBKDF2 via `PasswordHasher<T>`
- `POST /api/auth/registro` (público) cria o usuário como `Role.Admin` — permite que o primeiro cadastro acesse o sistema completo; Admins criam Operadores e Clientes via `POST /api/usuarios` (Admin only)
- `appsettings.json` contém strings vazias — segredos sobem via variáveis de ambiente
- Startup validation: `Program.cs` lança `InvalidOperationException` se `Jwt__Secret` ou `ConnectionStrings__DefaultConnection` estiverem ausentes/curtos
