# ParkingSystem — Contexto para Claude Code

## O que é este projeto

Sistema de gerenciamento de estacionamento em .NET 10, construído como projeto de aprendizado.
Arquitetura Modular Monolith com DDD e CQRS.

## Regra principal de desenvolvimento

**Implementar por partes, uma camada ou arquivo por vez.** O usuário está aprendendo — nunca gere toda uma camada de uma vez sem ser solicitado.

## Estrutura do projeto

```
src/
├── modules/
│   ├── identity/
│   │   ├── ParkingSystem.Module.Identity.Domain/
│   │   ├── ParkingSystem.Module.Identity.Infra.Data.EF/
│   │   └── ParkingSystem.Module.Identity.Application/
│   └── parking/
│       ├── ParkingSystem.Module.Parking.Domain/
│       ├── ParkingSystem.Module.Parking.Application/
│       └── ParkingSystem.Module.Parking.Infra.Data.EF/
├── services/
│   ├── webapi/   (Minimal APIs BFF)
│   ├── mobile/   (vazio — futuro)
│   └── workers/
│       ├── MigrateWorker/
│       ├── DataSeedWorker/
│       └── ReservaExpiracaoWorker/
└── shared/
    ├── ParkingSystem.Shared.Core/
    └── ParkingSystem.Shared.IoC/
tests/
├── ParkingSystem.Tests.UnitTests/
└── ParkingSystem.Tests.IntegrationTests/
```

## Convenções de código

- **Namespace raiz:** `ParkingSystem`
- **Framework:** .NET 10
- **Banco:** PostgreSQL 16 via Docker (porta 5432)
- **Tabelas:** snake_case (ex: `vagas`, `movimentacoes`)
- **JSON:** snake_case via `[JsonPropertyName]`
- **Sem comentários** salvo quando o "por quê" for não-óbvio
- **Sem controllers** — usar Minimal APIs com `MapGroup()` + static extension methods
- **EF Core** para escrita; **Dapper** para queries de leitura complexas
- **Soft delete** em todas as entidades que estendem `TrackableEntity`
- **CQRS:** `ICommandHandler<TCommand, ValidationResult>`, handlers são `internal`
- **`InternalsVisibleTo`** no `.csproj` de Application para WebApp, Shared.IoC e Tests

## Domínio — Entidades

| Entidade | Descrição |
|---|---|
| `Vaga` | Vaga física do estacionamento |
| `Movimentacao` | Entrada/saída de veículo |
| `Tarifa` | Valores por tipo de vaga (hora, diária, mensal) |
| `Reserva` | Agendamento antecipado de vaga |
| `Usuario` | Usuário do sistema (módulo Identity) |

## Roles

- `Admin` — acesso total
- `Operador` — entrada/saída/pagamento
- `Cliente` — reservas e consultas

## Regra de cálculo de tarifa

`TarifaService` na Application:
- < 15 min → gratuito
- `Math.Ceiling(horas) × ValorHora`
- Teto = `ValorDiaria`

## Segurança

- `appsettings.json`: valores vazios (`""`) para connection string e JWT secret
- `appsettings.Development.json`: valores reais para dev local (excluído pelo `.gitignore`)
- Produção: variáveis de ambiente `Jwt__Secret` e `ConnectionStrings__DefaultConnection`
- Novos registros via `POST /api/auth/registro` são **sempre** `Role.Cliente` — sem parâmetro de role

## Docker

```bash
docker compose up -d   # sobe o PostgreSQL
```

Connection string dev: `Host=localhost;Port=5432;Database=parking_system_db;Username=parking_admin;Password=SecretPassword123!`
