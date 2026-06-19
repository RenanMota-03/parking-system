# Modelo de Domínio

## Diagrama de Entidades

```
┌──────────────┐       ┌─────────────────┐
│    Tarifa    │       │     Usuario      │
│─────────────│       │─────────────────│
│ TipoVaga    │       │ Nome            │
│ ValorHora   │       │ Email (único)   │
│ ValorDiaria │       │ SenhaHash       │
│ ValorMensal │       │ Role            │
│ VigenteAte? │       └─────────────────┘
└──────────────┘                │
                                │ cria
       ┌────────────────────────▼──────┐
       │           Reserva             │
       │───────────────────────────────│
       │ UsuarioId                     │
       │ VagaId ──────────────┐        │
       │ DataAgendada         │        │
       │ DataLimite           │        │
       │ Status (enum)        │        │
       └──────────────────────┼────────┘
                              │
              ┌───────────────▼─────────────┐
              │            Vaga             │
              │────────────────────────────│
              │ Numero                     │
              │ TipoVaga (enum)            │
              │ Status (enum)              │
              └──────────┬─────────────────┘
                         │ ocupa
              ┌──────────▼─────────────────┐
              │        Movimentacao        │
              │────────────────────────────│
              │ VagaId                     │
              │ PlacaVeiculo               │
              │ DataEntrada                │
              │ DataSaida?                 │
              │ ValorTotal?                │
              │ Pago                       │
              │ FormaPagamento?            │
              └────────────────────────────┘
```

---

## Entidades

### `Vaga`

Representa uma vaga física do estacionamento. Encapsula sua própria máquina de estado.

**Campos**

| Campo | Tipo | Descrição |
|---|---|---|
| `Numero` | `string` | Identificador legível (ex: A1, B2) — máx 10 chars |
| `TipoVaga` | `TipoVaga` | Tipo físico da vaga |
| `Status` | `StatusVaga` | Estado atual da vaga |

**Máquina de Estado**

```
Disponivel ──Ocupar()──→ Ocupada ──Liberar()──→ Disponivel
     │                                               ↑
     └──Reservar()──→ Reservada ──LiberarReserva()──┘
     │
     └──IniciarManutencao()──→ Manutencao ──FinalizarManutencao()──→ Disponivel
```

**Enums**

```csharp
enum TipoVaga    { Carro, Moto, Caminhonete, DeficienteOuIdoso }
enum StatusVaga  { Disponivel, Ocupada, Reservada, Manutencao }
```

---

### `Movimentacao`

Registro do ciclo de vida de um veículo no estacionamento: entrada → saída → pagamento.

**Campos**

| Campo | Tipo | Descrição |
|---|---|---|
| `VagaId` | `long` | Vaga associada |
| `PlacaVeiculo` | `string` | Placa do veículo — máx 8 chars |
| `DataEntrada` | `DateTime` | Momento da entrada (UTC) |
| `DataSaida` | `DateTime?` | Preenchido em `RegistrarSaida()` |
| `ValorTotal` | `decimal?` | Calculado pelo `TarifaService` no checkout |
| `Pago` | `bool` | Indica pagamento concluído |
| `FormaPagamento` | `FormaPagamento?` | Preenchido em `Pagar()` |

**Métodos de domínio**

- `RegistrarSaida(decimal valorTotal)` — define `DataSaida` e `ValorTotal`
- `Pagar(FormaPagamento forma)` — marca como pago, valida que saída já ocorreu

**Enum**

```csharp
enum FormaPagamento { Dinheiro, Cartao, Pix }
```

---

### `Tarifa`

Define os valores cobrados por tipo de vaga. Pode ter validade (`VigenteAte`).

**Campos**

| Campo | Tipo | Descrição |
|---|---|---|
| `TipoVaga` | `TipoVaga` | Tipo ao qual a tarifa se aplica |
| `ValorHora` | `decimal` | Valor por hora (frações arredondadas para cima) |
| `ValorDiaria` | `decimal` | Teto diário — cobra-se o menor entre cálculo e diária |
| `ValorMensal` | `decimal` | Para contratos mensais |
| `VigenteAte` | `DateTime?` | `null` = vigente indefinidamente |

---

### `Reserva`

Agendamento antecipado de uma vaga por um cliente. Possui máquina de estado própria.

**Campos**

| Campo | Tipo | Descrição |
|---|---|---|
| `UsuarioId` | `string` | ID do cliente que reservou |
| `VagaId` | `long` | Vaga reservada |
| `DataAgendada` | `DateTime` | Início previsto da ocupação (deve ser futuro) |
| `DataLimite` | `DateTime` | Prazo máximo de chegada (deve ser > DataAgendada) |
| `Status` | `StatusReserva` | Estado atual da reserva |
| `Vaga` | `Vaga?` | Navigation property |

**Máquina de Estado**

```
Pendente ──Confirmar()──→ Confirmada ──Utilizar()──→ Utilizada
    │                         │
    └──Cancelar()──→ Cancelada│
                              └──Expirar()──→ Expirada
```

**Enum**

```csharp
enum StatusReserva { Pendente, Confirmada, Cancelada, Expirada, Utilizada }
```

**Invariantes de criação**

- `DataAgendada` deve ser posterior ao momento atual
- `DataLimite` deve ser posterior a `DataAgendada`
- `UsuarioId` não pode ser vazio

---

### `Usuario` (módulo Identity)

**Campos**

| Campo | Tipo | Descrição |
|---|---|---|
| `Nome` | `string` | Nome completo |
| `Email` | `string` | Normalizado para lowercase no construtor |
| `SenhaHash` | `string` | Hash PBKDF2 gerado pelo `PasswordHasher<T>` |
| `Role` | `Role` | Nível de acesso |

**Enum**

```csharp
enum Role { Admin, Operador, Cliente }
```

**Regra importante:** novos usuários criados via API são **sempre** `Role.Cliente`. Apenas via `DataSeedWorker` é possível criar um `Admin`.

---

## Serviços de Domínio

### `TarifaService`

Calcula o valor a ser cobrado por uma movimentação.

```
Entrada: TipoVaga, DataEntrada, DataSaida

1. duração = DataSaida - DataEntrada
2. se duração < 15 minutos → retorna 0 (carência)
3. busca tarifa vigente para o TipoVaga
4. horas = Math.Ceiling(duração.TotalHours)
5. valor = horas × tarifa.ValorHora
6. retorna Min(valor, tarifa.ValorDiaria)
```

---

## Regras de Negócio Cross-Cutting

### Detecção de Sobreposição de Reservas

Ao criar uma reserva, verifica se existe outra reserva ativa para a mesma vaga no mesmo intervalo:

```
sobreposição = reserva.DataAgendada < nova.DataLimite
            && reserva.DataLimite   > nova.DataAgendada
```

Se houver sobreposição, a operação é rejeitada com erro de validação.

### Soft Delete

Nenhuma entidade é deletada fisicamente. `TrackableEntity` adiciona `IsDeleted` que é setado como `true` em operações de remoção lógica. O EF Core aplica automaticamente `WHERE is_deleted = false` via global query filter. Queries Dapper devem adicionar `is_deleted = false` manualmente.

### UTC

Todas as datas são armazenadas e trabalhadas em UTC. O EF Core converte automaticamente via configuração `HasConversion`.
