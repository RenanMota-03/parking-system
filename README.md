# ParkingSystem

Sistema de gerenciamento de estacionamento construído com **.NET 10**, **PostgreSQL** e arquitetura **Modular Monolith + DDD + CQRS**.

Desenvolvido como projeto de aprendizado de boas práticas em engenharia de software.

---

## Stack Tecnológica

| Camada | Tecnologia |
|---|---|
| Backend | .NET 10 / ASP.NET Core (Minimal APIs) |
| Banco de Dados | PostgreSQL 16 (Docker) |
| ORM (escrita) | Entity Framework Core 10 (Code-First) |
| Queries (leitura) | Dapper |
| Autenticação | JWT Bearer (HMAC-SHA256) + PasswordHasher (PBKDF2) |
| Documentação API | Scalar (OpenAPI) |
| Testes unitários | xUnit + NSubstitute |
| Testes integração | xUnit + WebApplicationFactory |
| Frontend Web | Vue 3 + Vite + TypeScript + PrimeVue 4.2.5 + Pinia + Axios |
| Mobile | _(planejado)_ |

---

## Arquitetura

O projeto segue o padrão **Modular Monolith** com **DDD** e **CQRS**. Cada módulo é autossuficiente com suas próprias camadas Domain → Application → Infra.

```
src/
├── modules/
│   ├── identity/                                         # Autenticação e usuários
│   │   ├── ParkingSystem.Module.Identity.Domain/
│   │   ├── ParkingSystem.Module.Identity.Infra.Data.EF/
│   │   └── ParkingSystem.Module.Identity.Application/
│   └── parking/                                          # Domínio principal
│       ├── ParkingSystem.Module.Parking.Domain/
│       ├── ParkingSystem.Module.Parking.Application/
│       └── ParkingSystem.Module.Parking.Infra.Data.EF/
├── services/
│   ├── webapi/    # API REST — Minimal APIs, BFF
│   ├── webui/     # SPA Vue 3 — interface administrativa
│   ├── mobile/    # (planejado)
│   └── workers/
│       ├── MigrateWorker/            # Executa migrations no startup
│       ├── DataSeedWorker/           # Seed de dados iniciais
│       └── ReservaExpiracaoWorker/   # Background service — expira reservas vencidas
└── shared/
    ├── ParkingSystem.Shared.Core/   # Abstrações DDD/CQRS
    └── ParkingSystem.Shared.IoC/    # Bootstrap de Injeção de Dependência

tests/
├── ParkingSystem.Tests.UnitTests/        # Testes de domínio e handlers (73 testes)
└── ParkingSystem.Tests.IntegrationTests/ # Testes HTTP end-to-end
```

> Documentação arquitetural detalhada em [`docs/architecture/`](docs/architecture/overview.md).

---

## Como Rodar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Node.js 20+](https://nodejs.org/) (para a WebUI)

### 1. Subir o banco de dados

```bash
docker compose up -d
```

### 2. Executar as migrations

```bash
dotnet run --project src/services/workers/MigrateWorker
```

### 3. Popular dados iniciais

```bash
# Requer a variável Seed__AdminSenha
$env:Seed__AdminSenha="SuaSenhaAdmin@123"
dotnet run --project src/services/workers/DataSeedWorker
```

### 4. Iniciar a API

```bash
dotnet run --project src/services/webapi/ParkingSystem.WebApi.Bff.WebApp
```

### 5. Iniciar a WebUI

```bash
cd src/services/webui
npm install
npm run dev
```

Abre em `http://localhost:5173`. Usa o usuário `admin@parking.com` criado pelo DataSeedWorker.

### 6. Acessar a documentação interativa da API

```
http://localhost:5000/reference
```

### 7. Rodar os testes

```bash
# Testes unitários (sem dependência externa)
dotnet test tests/ParkingSystem.Tests.UnitTests

# Testes de integração (requer PostgreSQL rodando)
dotnet test tests/ParkingSystem.Tests.IntegrationTests
```

---

## Endpoints da API

| Verbo | Endpoint | Descrição | Role |
|---|---|---|---|
| `POST` | `/api/auth/registro` | Cadastro — cria usuário `Admin` | Público |
| `POST` | `/api/auth/login` | Login — retorna JWT | Público |
| `GET` | `/api/usuarios` | Lista todos os usuários (paginado) | Admin |
| `POST` | `/api/usuarios` | Cria Operador ou Cliente | Admin |
| `GET` | `/api/vagas` | Lista vagas com filtro e paginação | Autenticado |
| `POST` | `/api/vagas` | Cadastra nova vaga | Admin |
| `PATCH` | `/api/vagas/{id}/status` | Altera status da vaga | Admin |
| `POST` | `/api/fluxo/entrada` | Registra entrada de veículo | Admin, Operador |
| `POST` | `/api/fluxo/saida` | Registra saída e calcula valor | Admin, Operador |
| `POST` | `/api/fluxo/pagamento` | Processa pagamento | Admin, Operador |
| `GET` | `/api/movimentacoes` | Lista todas as movimentações (paginado) | Admin, Operador |
| `GET` | `/api/movimentacoes/{id}` | Busca movimentação por id | Admin, Operador |
| `GET` | `/api/tarifas` | Lista tarifas vigentes | Admin, Operador |
| `GET` | `/api/tarifas/{id}` | Busca tarifa por id | Admin, Operador |
| `POST` | `/api/tarifas` | Cadastra nova tarifa | Admin |
| `PUT` | `/api/tarifas/{id}` | Atualiza valores de tarifa | Admin |
| `GET` | `/api/reservas` | Lista todas as reservas (paginado) | Admin |
| `GET` | `/api/reservas/{id}` | Busca reserva por id | Admin, Cliente |
| `POST` | `/api/reservas` | Cria nova reserva | Admin, Cliente |
| `DELETE` | `/api/reservas/{id}` | Cancela reserva | Admin, Cliente |

> Referência completa com corpos de requisição/resposta em [`docs/api/endpoints.md`](docs/api/endpoints.md).

---

## Variáveis de Ambiente (Produção)

| Variável | Descrição |
|---|---|
| `ConnectionStrings__DefaultConnection` | Connection string do PostgreSQL |
| `Jwt__Secret` | Chave secreta JWT (mínimo 32 caracteres) |
| `Seed__AdminSenha` | Senha do usuário Admin (DataSeedWorker) |

> Guia completo em [`docs/deployment/configuration.md`](docs/deployment/configuration.md).

---

## Documentação

| Documento | Descrição |
|---|---|
| [`docs/architecture/overview.md`](docs/architecture/overview.md) | Visão geral da arquitetura |
| [`docs/architecture/domain-model.md`](docs/architecture/domain-model.md) | Modelo de domínio — entidades, enums, regras |
| [`docs/api/endpoints.md`](docs/api/endpoints.md) | Referência completa dos endpoints |
| [`docs/development/getting-started.md`](docs/development/getting-started.md) | Primeiros passos |
| [`docs/development/conventions.md`](docs/development/conventions.md) | Padrões e convenções de código |
| [`docs/development/testing.md`](docs/development/testing.md) | Estratégia e guia de testes |
| [`docs/deployment/configuration.md`](docs/deployment/configuration.md) | Configuração e variáveis de ambiente |
