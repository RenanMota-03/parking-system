# Referência de Endpoints

Base URL: `http://localhost:5000`

Documentação interativa (Scalar): `http://localhost:5000/reference`

## Autenticação

Endpoints protegidos requerem o header:

```
Authorization: Bearer <token>
```

O token é obtido via `POST /api/auth/login`.

---

## Auth

### `POST /api/auth/registro`

Registra um novo usuário. Role é sempre `Admin`.

> **Nota de desenvolvimento:** o primeiro cadastro vira Admin, que pode criar Operadores e Clientes via `POST /api/usuarios`.

**Request**

```json
{
  "nome": "Maria Silva",
  "email": "maria@exemplo.com",
  "senha": "Senha@123"
}
```

**Validações**
- `nome`: obrigatório
- `email`: obrigatório, deve ser único
- `senha`: mínimo 6 caracteres

**Response `201 Created`**

```json
{
  "id": 1,
  "email": "maria@exemplo.com",
  "role": "Admin"
}
```

**Response `422 Unprocessable Entity`** — erros de validação

---

### `POST /api/auth/login`

Autentica o usuário e retorna um JWT.

**Request**

```json
{
  "email": "maria@exemplo.com",
  "senha": "Senha@123"
}
```

**Response `200 OK`**

```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_at": "2025-06-18T20:00:00Z",
  "role": "Cliente",
  "name": "Maria Silva"
}
```

**Response `401 Unauthorized`** — credenciais inválidas

---

## Usuários

### `GET /api/usuarios`

Lista todos os usuários com paginação.

**Auth:** Admin

**Query params:** `page` (padrão 1), `pageSize` (padrão 20)

**Response `200 OK`** — `PagedResult<UsuarioListDto>`

```json
{
  "items": [
    {
      "id": 1,
      "nome": "Admin Sistema",
      "email": "admin@parking.com",
      "role": 0,
      "created_at": "2025-06-18T10:00:00Z"
    }
  ],
  "page": 1,
  "page_size": 20,
  "total_items": 1,
  "total_pages": 1,
  "has_next_page": false,
  "has_previous_page": false
}
```

`role`: 0=Admin, 1=Operador, 2=Cliente

---

### `POST /api/usuarios`

Cria um novo usuário. Usado pelo Admin para cadastrar Operadores e Clientes.

**Auth:** Admin

**Request**

```json
{
  "nome": "João da Silva",
  "email": "joao@exemplo.com",
  "senha": "Senha@123",
  "role": 1
}
```

`role`: 1=Operador, 2=Cliente (Admin não deve ser criado por esta rota em produção)

**Validações**
- `nome`: obrigatório
- `email`: obrigatório, deve ser único
- `senha`: mínimo 6 caracteres
- `role`: obrigatório

**Response `201 Created`**

```json
{
  "id": 2,
  "email": "joao@exemplo.com",
  "role": 1
}
```

**Response `401 Unauthorized`** — sem token  
**Response `403 Forbidden`** — token de Operador ou Cliente  
**Response `422 Unprocessable Entity`** — erros de validação

---

## Vagas

### `GET /api/vagas`

Lista vagas com filtro opcional por status e paginação.

**Auth:** Qualquer role autenticado

**Query params**

| Param | Tipo | Padrão | Descrição |
|---|---|---|---|
| `status` | `int?` | — | Filtra por StatusVaga (0=Disponivel, 1=Ocupada, 2=Reservada, 3=Manutencao) |
| `page` | `int` | `1` | Página atual |
| `pageSize` | `int` | `20` | Itens por página |

**Response `200 OK`**

```json
{
  "items": [
    {
      "id": 1,
      "numero": "A1",
      "tipo_vaga": 0,
      "status": 0
    }
  ],
  "page": 1,
  "page_size": 20,
  "total_items": 9,
  "total_pages": 1,
  "has_next_page": false,
  "has_previous_page": false
}
```

---

### `POST /api/vagas`

Cadastra uma nova vaga.

**Auth:** Admin

**Request**

```json
{
  "numero": "E1",
  "tipoVaga": 0
}
```

`tipoVaga`: 0=Carro, 1=Moto, 2=Caminhonete, 3=DeficienteOuIdoso

**Response `201 Created`**

```json
{ "id": 10, "numero": "E1" }
```

**Response `422 Unprocessable Entity`** — número já existe ou validação falhou

---

### `PATCH /api/vagas/{id}/status`

Altera o status de uma vaga (ex: colocar em manutenção).

**Auth:** Admin

**Request**

```json
{ "novoStatus": 3 }
```

**Response `200 OK`**

```json
{ "id": 1 }
```

---

## Fluxo de Veículos

### `POST /api/fluxo/entrada`

Registra a entrada de um veículo no estacionamento. Ocupa a vaga automaticamente.

**Auth:** Admin, Operador

**Request**

```json
{
  "vagaId": 1,
  "placaVeiculo": "ABC1D23"
}
```

**Validações**
- Vaga deve existir
- Placa não pode ter outra entrada em aberto

**Response `201 Created`**

```json
{
  "id": 42,
  "placa": "ABC1D23",
  "data_entrada": "2025-06-18T10:30:00Z"
}
```

---

### `POST /api/fluxo/saida`

Registra a saída do veículo e calcula o valor devido. Libera a vaga automaticamente.

**Auth:** Admin, Operador

**Request**

```json
{ "placaVeiculo": "ABC1D23" }
```

**Validações**
- Deve existir entrada em aberto para a placa

**Response `200 OK`** — retorna a movimentação completa com `valor_total`

---

### `POST /api/fluxo/pagamento`

Processa o pagamento e encerra a movimentação.

**Auth:** Admin, Operador

**Request**

```json
{
  "movimentacaoId": 42,
  "formaPagamento": 1
}
```

`formaPagamento`: 0=Dinheiro, 1=Cartao, 2=Pix

**Response `200 OK`**

```json
{ "id": 42 }
```

---

## Movimentações

### `GET /api/movimentacoes`

Lista todas as movimentações (incluindo as com saída já registrada), com paginação.

**Auth:** Admin, Operador

**Query params:** `page`, `pageSize`

**Response `200 OK`** — `PagedResult<MovimentacaoDto>`

```json
{
  "items": [
    {
      "id": 1,
      "vaga_id": 2,
      "numero_vaga": "A2",
      "placa_veiculo": "ABC1D23",
      "data_entrada": "2025-06-18T08:00:00Z",
      "data_saida": "2025-06-18T10:00:00Z",
      "valor_total": 16.00,
      "pago": true,
      "forma_pagamento": 1
    }
  ],
  ...
}
```

Movimentações em aberto têm `data_saida: null`, `valor_total: null` e `pago: false`.

---

### `GET /api/movimentacoes/{id}`

Busca movimentação por id.

**Auth:** Admin, Operador

**Response `200 OK`** ou `404 Not Found`

---

## Tarifas

### `GET /api/tarifas`

Lista todas as tarifas.

**Auth:** Admin, Operador

**Response `200 OK`**

```json
[
  {
    "id": 1,
    "tipo_vaga": 0,
    "valor_hora": 8.00,
    "valor_diaria": 50.00,
    "valor_mensal": 600.00,
    "vigente_ate": null
  }
]
```

---

### `GET /api/tarifas/{id}`

Busca tarifa por id.

**Auth:** Admin, Operador

---

### `POST /api/tarifas`

Cadastra nova tarifa.

**Auth:** Admin

**Request**

```json
{
  "tipoVaga": 0,
  "valorHora": 10.00,
  "valorDiaria": 60.00,
  "valorMensal": 700.00,
  "vigenteAte": null
}
```

---

### `PUT /api/tarifas/{id}`

Atualiza valores de uma tarifa existente.

**Auth:** Admin

**Request**

```json
{
  "valorHora": 12.00,
  "valorDiaria": 70.00,
  "valorMensal": 800.00,
  "vigenteAte": "2025-12-31T23:59:59Z"
}
```

---

## Reservas

### `GET /api/reservas`

Lista todas as reservas com paginação.

**Auth:** Admin

**Query params:** `page`, `pageSize`

**Response `200 OK`** — `PagedResult<ReservaDto>`

---

### `GET /api/reservas/{id}`

Busca reserva por id.

**Auth:** Admin, Cliente

---

### `POST /api/reservas`

Cria nova reserva para uma vaga.

**Auth:** Admin, Cliente

**Request**

```json
{
  "vagaId": 3,
  "usuarioId": "user-uuid-aqui",
  "dataAgendada": "2025-06-20T09:00:00Z",
  "dataLimite": "2025-06-20T09:30:00Z"
}
```

**Validações**
- Vaga deve existir e estar disponível para reserva
- `DataAgendada` deve ser futura
- `DataLimite` deve ser posterior a `DataAgendada`
- Não pode haver sobreposição com outra reserva ativa na mesma vaga

**Response `201 Created`** — retorna a `ReservaDto` completa

---

### `DELETE /api/reservas/{id}`

Cancela uma reserva ativa. Libera a vaga de volta para `Disponivel`.

**Auth:** Admin, Cliente

**Response `204 No Content`**

**Response `422 Unprocessable Entity`** — reserva já cancelada/expirada/utilizada
