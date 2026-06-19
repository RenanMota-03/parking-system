# Convenções e Padrões de Código

## Nomenclatura

| Artefato | Convenção | Exemplo |
|---|---|---|
| Namespace raiz | `ParkingSystem` | `ParkingSystem.Module.Parking.Domain` |
| Entidades | PascalCase | `Vaga`, `Movimentacao` |
| Commands | `<Acao><Entidade>Command` | `CadastrarVagaCommand` |
| Handlers | `<Command>Handler` (interno ao arquivo do command) | `CadastrarVagaCommandHandler` |
| Queries (interface) | `I<Entidade>Queries` | `IVagaQueries` |
| Repositórios (interface) | `I<Entidade>Repository` | `IVagaRepository` |
| DTOs | `<Entidade>Dto` | `VagaDto` |
| Endpoints | `<Entidade>Endpoints` | `VagaEndpoints` |
| Tabelas (banco) | snake_case | `vagas`, `movimentacoes` |
| Colunas (banco) | snake_case | `placa_veiculo`, `data_entrada` |
| JSON fields | snake_case via `[JsonPropertyName]` | `"data_entrada"` |

---

## Estrutura de um Command

Command e Handler vivem no mesmo arquivo. O Handler é `internal` e o Command é `public`.

```csharp
// CadastrarVagaCommand.cs
public class CadastrarVagaCommand(string numero, TipoVaga tipoVaga) : Command
{
    public string Numero { get; } = numero;
    public TipoVaga TipoVaga { get; } = tipoVaga;

    public override bool IsValid()
    {
        ValidationResult = new CadastrarVagaCommandValidator().Validate(this);
        return ValidationResult.IsValid;
    }
}

internal class CadastrarVagaCommandHandler(IVagaRepository vagaRepository)
    : CommandHandler<CadastrarVagaCommand>
{
    public override async Task<ValidationResult> Handle(CadastrarVagaCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.IsValid()) return command.ValidationResult!;

        // regras de negócio...

        await vagaRepository.AddAsync(vaga, cancellationToken);
        var result = await PersistData(vagaRepository.UnitOfWork);

        if (result.IsValid)
            result.Data = new { id = vaga.Id, numero = vaga.Numero };

        return result;
    }
}

public class CadastrarVagaCommandValidator : ValidatorBase<CadastrarVagaCommand>
{
    public CadastrarVagaCommandValidator()
    {
        ValidateNotEmpty(x => x.Numero, "O número da vaga é obrigatório.");
        ValidateMaxLength(x => x.Numero, 10, "O número da vaga deve ter no máximo 10 caracteres.");
    }
}
```

**Regras:**
- Um único `PersistData()` por handler (não há commits parciais)
- `result.Data` recebe um objeto anônimo com os dados de retorno
- Handlers não recebem `ILogger` — erros de infraestrutura propagam exceção

---

## Estrutura de uma Query (Dapper)

```csharp
// VagaQueries.cs
public interface IVagaQueries
{
    Task<PagedResult<VagaDto>> GetAllAsync(int? status, int page, int pageSize);
}

public class VagaQueries(IDbConnection db) : IVagaQueries
{
    public async Task<PagedResult<VagaDto>> GetAllAsync(int? status, int page, int pageSize)
    {
        const string where = "WHERE v.is_deleted = false";
        // ...adicionar filtros dinâmicos

        var count = await db.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM vagas v {where}", parameters);
        var items = await db.QueryAsync<VagaDto>($"SELECT ... FROM vagas v {where} LIMIT @PageSize OFFSET @Offset", parameters);

        return new PagedResult<VagaDto>(items, count, page, pageSize);
    }
}
```

**Regras:**
- `is_deleted = false` **sempre** explícito — o EF global filter não se aplica ao Dapper
- Constante SQL compartilhada entre a query de `COUNT(*)` e a de `SELECT` para evitar dessincronização
- `DynamicParameters` para parâmetros opcionais/dinâmicos
- DTOs com `[JsonPropertyName("snake_case")]` em todas as propriedades

---

## Estrutura de um Endpoint (Minimal API)

```csharp
// VagaEndpoints.cs
public static class VagaEndpoints
{
    public static void MapVagaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vagas").WithTags("Vagas");

        group.MapGet("/", ListVagasAsync)
             .WithSummary("Lista todas as vagas")
             .RequireAuthorization();

        group.MapPost("/", CadastrarVagaAsync)
             .WithSummary("Cadastra uma nova vaga")
             .RequireAuthorization(p => p.RequireRole("Admin"));
    }

    private static async Task<IResult> ListVagasAsync(
        [FromServices] IVagaQueries queries,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Results.Ok(await queries.GetAllAsync(null, page, pageSize));
    }

    private static async Task<IResult> CadastrarVagaAsync(
        [FromBody] CadastrarVagaRequest request,
        [FromServices] ICommandHandler<CadastrarVagaCommand, ValidationResult> handler)
    {
        var result = await handler.Handle(new CadastrarVagaCommand(request.Numero, request.TipoVaga));
        if (!result.IsValid) return Results.UnprocessableEntity(result.Errors);
        return Results.Created($"/api/vagas/{result.Data?.id}", result.Data);
    }

    public record CadastrarVagaRequest(string Numero, TipoVaga TipoVaga);
}
```

**Regras:**
- Sem controllers — sempre `MapGroup()` + static extension methods
- Records de request definidos ao final do arquivo
- Erros de validação retornam `422 Unprocessable Entity` (não `400 Bad Request`)
- `RequireAuthorization()` em todos os endpoints — explícito, sem "authorize por padrão"

---

## Entidades de Domínio

```csharp
public class Vaga : TrackableEntity
{
    public string Numero { get; private set; }
    public TipoVaga TipoVaga { get; private set; }
    public StatusVaga Status { get; private set; }

    public Vaga(string numero, TipoVaga tipoVaga)
    {
        DomainValidation.NotNullOrEmpty(numero, nameof(Numero));
        DomainValidation.MaxLength(numero, 10, nameof(Numero));

        Numero = numero;
        TipoVaga = tipoVaga;
        Status = StatusVaga.Disponivel;
    }

    public void Ocupar()
    {
        DomainValidation.That(Status == StatusVaga.Disponivel, "Vaga não está disponível.");
        Status = StatusVaga.Ocupada;
    }
}
```

**Regras:**
- Setters são `private` — estado só muda via métodos de domínio
- Validações no construtor via `DomainValidation` (lança `EntityValidationException`)
- Herdar de `TrackableEntity` para soft delete e auditoria automática

---

## Registro de Dependências (IoC)

Todos os serviços são registrados em `NativeInjectorBootStrapper.RegisterServices()` em `Shared.IoC`. Adicionar aqui ao criar novos handlers, repositories ou queries.

```csharp
// NativeInjectorBootStrapper.cs
services.AddScoped<IVagaRepository, VagaRepository>();
services.AddScoped<IVagaQueries, VagaQueries>();
services.AddScoped<ICommandHandler<CadastrarVagaCommand, ValidationResult>, CadastrarVagaCommandHandler>();
```

---

## Comentários

Comentários são usados **somente** quando o *porquê* é não-óbvio: uma limitação oculta, um workaround para um bug específico, ou comportamento que surpreenderia um leitor. Identificadores bem nomeados já documentam o *o quê*.

---

## Migrations

Ao alterar entidades ou configurações EF, crie uma migration nova:

```bash
# Parking
dotnet ef migrations add <NomeDaMigration> \
  --project src/modules/parking/ParkingSystem.Module.Parking.Infra.Data.EF \
  --startup-project src/services/workers/MigrateWorker \
  --context ParkingDbContext

# Identity
dotnet ef migrations add <NomeDaMigration> \
  --project src/modules/identity/ParkingSystem.Module.Identity.Infra.Data.EF \
  --startup-project src/services/workers/MigrateWorker \
  --context IdentityDbContext
```

Nunca modifique migrations já aplicadas em ambientes compartilhados — sempre crie uma nova.
