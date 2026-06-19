# Configuração e Variáveis de Ambiente

## Estratégia de Configuração

O projeto segue a hierarquia de configuração do ASP.NET Core:

```
appsettings.json          ← valores padrão (strings vazias para segredos)
appsettings.{Env}.json    ← sobrescreve por ambiente
Variáveis de ambiente     ← sobrescreve tudo (mais alta prioridade)
```

### `appsettings.json` (commitado)

Contém a estrutura de configuração com valores vazios para segredos. Nunca deve conter credenciais reais.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "Secret": "",
    "Issuer": "ParkingSystem",
    "Audience": "ParkingSystemClients",
    "ExpiresInHours": 8
  }
}
```

### `appsettings.Development.json` (não commitado)

Contém os valores reais para desenvolvimento local. Excluído pelo `.gitignore`.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=parking_system_db;Username=parking_admin;Password=SecretPassword123!"
  },
  "Jwt": {
    "Secret": "parking-system-dev-secret-key-256bits-minimum!"
  }
}
```

---

## Variáveis de Ambiente

Em produção, **todos os segredos são fornecidos via variáveis de ambiente**. O .NET reconhece `__` (duplo underscore) como separador de seção.

### API — `ParkingSystem.WebApi.Bff.WebApp`

| Variável | Seção equivalente | Obrigatória | Descrição |
|---|---|---|---|
| `ConnectionStrings__DefaultConnection` | `ConnectionStrings:DefaultConnection` | Sim | Connection string completa do PostgreSQL |
| `Jwt__Secret` | `Jwt:Secret` | Sim | Chave secreta HMAC-SHA256 (mínimo 32 caracteres) |
| `Jwt__Issuer` | `Jwt:Issuer` | Não | Emissor do token (padrão: `ParkingSystem`) |
| `Jwt__Audience` | `Jwt:Audience` | Não | Audiência do token (padrão: `ParkingSystemClients`) |
| `Jwt__ExpiresInHours` | `Jwt:ExpiresInHours` | Não | Validade do token em horas (padrão: `8`) |
| `ASPNETCORE_ENVIRONMENT` | — | Não | Ambiente: `Development`, `Production` |
| `ASPNETCORE_URLS` | — | Não | Ex: `http://+:8080` para alterar a porta |

### Workers

| Variável | Worker | Obrigatória | Descrição |
|---|---|---|---|
| `ConnectionStrings__DefaultConnection` | MigrateWorker, DataSeedWorker | Sim | Connection string do PostgreSQL |
| `Seed__AdminSenha` | DataSeedWorker | Sim | Senha do usuário administrador criado no seed |
| `Seed__AdminEmail` | DataSeedWorker | Não | Email do admin (padrão: `admin@parking.com`) |
| `Seed__AdminNome` | DataSeedWorker | Não | Nome do admin (padrão: `Administrador`) |
| `ReservaExpiracao__IntervaloEmMinutos` | ReservaExpiracaoWorker | Não | Intervalo de verificação (padrão: `5`) |

---

## Validação de Startup

A API valida os segredos obrigatórios na inicialização e **lança `InvalidOperationException`** se estiverem ausentes ou inválidos:

```csharp
// Program.cs
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection não configurada. " +
        "Em produção, defina a variável de ambiente ConnectionStrings__DefaultConnection.");

if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Secret não configurado ou muito curto (mínimo 32 caracteres). " +
        "Em produção, defina a variável de ambiente Jwt__Secret.");
```

Isso garante falha rápida (fail-fast) em vez de erros obscuros em runtime.

Os workers fazem validação equivalente e retornam **exit code 1** em caso de erro:

```bash
ERRO: ConnectionStrings__DefaultConnection não configurada.
# processo termina com exit code 1
```

---

## Configurando em Docker / Docker Compose

### Exemplo `docker-compose.override.yml` (produção local)

```yaml
services:
  webapi:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=parking_system_db;Username=parking_admin;Password=SenhaProducao!
      - Jwt__Secret=chave-super-secreta-com-mais-de-32-caracteres!
      - ASPNETCORE_URLS=http://+:8080

  migrate:
    environment:
      - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=parking_system_db;Username=parking_admin;Password=SenhaProducao!

  dataseed:
    environment:
      - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=parking_system_db;Username=parking_admin;Password=SenhaProducao!
      - Seed__AdminSenha=AdminSenhaForte@2025
```

---

## Boas Práticas de Segurança

1. **Nunca commite `appsettings.Development.json`** — ele está no `.gitignore` e deve assim permanecer.
2. **JWT Secret deve ter no mínimo 32 caracteres** — o startup valida isso.
3. **Use senhas fortes para o banco** — especialmente em ambientes acessíveis externamente.
4. **Rotacione o JWT Secret** periodicamente em produção — ao rotacionar, todos os tokens emitidos antes são invalidados.
5. **Não use a mesma senha do admin** em todos os ambientes — cada ambiente deve ter suas próprias credenciais.

---

## Gerando um JWT Secret seguro

```bash
# PowerShell
[System.Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))

# Bash / Linux
openssl rand -base64 32

# Node.js
node -e "console.log(require('crypto').randomBytes(32).toString('base64'))"
```
