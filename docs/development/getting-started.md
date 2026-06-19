# Primeiros Passos

## Pré-requisitos

| Ferramenta | Versão mínima | Download |
|---|---|---|
| .NET SDK | 10.0 | https://dotnet.microsoft.com/download |
| Node.js | 20.0 | https://nodejs.org/ |
| Docker Desktop | 4.x | https://www.docker.com/products/docker-desktop/ |
| Git | qualquer | https://git-scm.com |

Ferramentas opcionais (recomendadas):

- **IDE:** Visual Studio 2022 17.10+, Rider 2024.1+ ou VS Code com extensão C# Dev Kit
- **Cliente HTTP:** Bruno, Insomnia ou Postman (para testar a API diretamente)
- **DBeaver / pgAdmin:** para inspecionar o banco

---

## 1. Clonar o repositório

```bash
git clone <url-do-repositório>
cd projeto-estacionamento
```

---

## 2. Subir o banco de dados

```bash
docker compose up -d
```

Isso inicia o PostgreSQL 16 na porta `5432` com:

| Parâmetro | Valor |
|---|---|
| Host | `localhost` |
| Porta | `5432` |
| Database | `parking_system_db` |
| Usuário | `parking_admin` |
| Senha | `SecretPassword123!` |

Para verificar se está rodando:

```bash
docker compose ps
```

---

## 3. Configurar segredos de desenvolvimento

O arquivo `appsettings.Development.json` não está no repositório (excluído pelo `.gitignore`). Crie-o manualmente:

```
src/services/webapi/ParkingSystem.WebApi.Bff.WebApp/appsettings.Development.json
```

Conteúdo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=parking_system_db;Username=parking_admin;Password=SecretPassword123!"
  },
  "Jwt": {
    "Secret": "parking-system-dev-secret-key-256bits-minimum!"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

---

## 4. Executar as migrations

```bash
dotnet run --project src/services/workers/MigrateWorker
```

Isso cria todas as tabelas no banco via `MigrateAsync()` nos dois DbContexts (`IdentityDbContext` e `ParkingDbContext`).

---

## 5. Seed de dados iniciais

```bash
# PowerShell
$env:Seed__AdminSenha="Admin@123"
dotnet run --project src/services/workers/DataSeedWorker

# Bash
Seed__AdminSenha="Admin@123" dotnet run --project src/services/workers/DataSeedWorker
```

Isso cria:
- Usuário admin (`admin@parking.com` / senha definida em `Seed__AdminSenha`)
- 4 tarifas (Carro, Moto, Caminhonete, Deficiente/Idoso)
- 9 vagas (A1–A4 Carro, B1–B2 Moto, C1 Caminhonete, D1–D2 Deficiente)
- 17 movimentações históricas pagas (dados para os gráficos do Relatório Financeiro)
- 3 entradas em aberto do dia atual (veículos presentes no estacionamento)

O seed é **idempotente** — pode ser executado múltiplas vezes sem duplicar dados.

---

## 6. Iniciar a API

```bash
dotnet run --project src/services/webapi/ParkingSystem.WebApi.Bff.WebApp
```

A API sobe em `http://localhost:5000` (porta pode variar — observe o output do terminal).

---

## 7. Iniciar a WebUI

```bash
cd src/services/webui
npm install   # apenas na primeira vez
npm run dev
```

A interface administrativa abre em `http://localhost:5173`. Faça login com o usuário criado pelo DataSeedWorker:

- **E-mail:** `admin@parking.com`
- **Senha:** valor definido em `Seed__AdminSenha` (ex.: `Admin@123`)

---

## 8. Acessar a documentação interativa da API

Abra no navegador:

```
http://localhost:5000/reference
```

A interface Scalar permite explorar todos os endpoints, ver schemas de request/response e executar chamadas diretamente.

---

## 9. Rodar os testes

```bash
# Todos os testes unitários (sem banco, roda offline)
dotnet test tests/ParkingSystem.Tests.UnitTests

# Testes de integração (requer PostgreSQL rodando via docker compose)
dotnet test tests/ParkingSystem.Tests.IntegrationTests

# Todos de uma vez
dotnet test
```

Os testes de integração criam e destroem automaticamente um banco separado (`parking_system_integration_test_db`).

---

## Fluxo básico de uso

### Via WebUI (recomendado)

1. Abra `http://localhost:5173`
2. Login com `admin@parking.com` / `Admin@123`
3. **Dashboard** — KPIs em tempo real (ocupação, receita, reservas ativas)
4. **Fluxo (Entrada/Saída)** — registre a entrada de um veículo, depois feche o ticket e processe o pagamento
5. **Vagas** — mapa visual de todas as vagas com status em tempo real
6. **Reservas** — agende uma vaga e acompanhe o status
7. **Relatórios** — gráfico financeiro filtrado por período
8. **Configurações** — cadastre Operadores e Clientes, ajuste parâmetros do sistema

### Via API direta

```
1. POST /api/auth/login  (email: admin@parking.com, senha: Admin@123)
   → copiar o access_token

2. GET  /api/vagas        (Authorization: Bearer <token>)
   → ver vagas disponíveis

3. POST /api/fluxo/entrada  { "vagaId": 1, "placaVeiculo": "ABC1234" }
   → entrada registrada, vaga marcada como Ocupada

4. POST /api/fluxo/saida    { "placaVeiculo": "ABC1234" }
   → valor calculado

5. POST /api/fluxo/pagamento  { "movimentacaoId": 1, "formaPagamento": 1 }
   → pagamento processado, vaga liberada
```

---

## Problemas Comuns

**`dotnet run` falha com erro de connection string:**
Verifique se o `appsettings.Development.json` existe e se o Docker está rodando (`docker compose ps`).

**Porta 5432 em uso:**
Verifique se há outro PostgreSQL rodando localmente. Altere a porta no `docker-compose.yml` se necessário.

**`MSB3027` ao buildar (arquivo em uso):**
A API está rodando em segundo plano. Encerre o processo `ParkingSystem.WebApi.Bff.WebApp` antes de buildar.

**Testes de integração falham com erro de conexão:**
O banco de teste precisa estar acessível. Confirme com `docker compose ps`.
