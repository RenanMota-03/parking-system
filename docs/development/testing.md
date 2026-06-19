# Estratégia e Guia de Testes

## Visão Geral

O projeto possui dois projetos de teste com responsabilidades distintas:

| Projeto | Tipo | Dependências externas | Cobertura |
|---|---|---|---|
| `ParkingSystem.Tests.UnitTests` | Unitários | Nenhuma | Entidades, handlers, services |
| `ParkingSystem.Tests.IntegrationTests` | Integração | PostgreSQL | Endpoints HTTP end-to-end |

---

## Testes Unitários

### Como rodar

```bash
dotnet test tests/ParkingSystem.Tests.UnitTests
```

Não requer banco de dados nem Docker. Roda completamente offline.

### Ferramentas

- **xUnit** — framework de testes
- **NSubstitute** — mocking de interfaces (repositórios, UoW)

### Estrutura

```
tests/ParkingSystem.Tests.UnitTests/
├── Parking/
│   ├── TarifaServiceTests.cs          # Lógica de cálculo de tarifa
│   ├── VagaTests.cs                   # Máquina de estado da Vaga
│   ├── ReservaTests.cs                # Máquina de estado da Reserva
│   ├── CadastrarVagaCommandHandlerTests.cs
│   ├── RegistrarEntradaCommandHandlerTests.cs
│   └── RegistrarSaidaCommandHandlerTests.cs
└── Identity/
    ├── UsuarioTests.cs
    └── RegistrarUsuarioCommandHandlerTests.cs
```

### Padrão de teste

Cada classe de teste usa o padrão **Arrange / Act / Assert** e nomes descritivos no formato:

```
<Método>_<Cenário>_<ResultadoEsperado>
```

Exemplo:

```csharp
[Fact]
public async Task Handle_NumeroJaExiste_RetornaErro()
{
    // Arrange
    _repo.GetByNumeroAsync("A1", Arg.Any<CancellationToken>())
         .Returns(new Vaga("A1", TipoVaga.Carro));
    var command = new CadastrarVagaCommand("A1", TipoVaga.Carro);

    // Act
    var result = await _sut.Handle(command);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("A1"));
    await _repo.DidNotReceive().AddAsync(Arg.Any<Vaga>(), Arg.Any<CancellationToken>());
}
```

### Setup do handler

Cada classe de handler-test cria mocks via construtor e configura o UoW para retornar sucesso:

```csharp
public class CadastrarVagaCommandHandlerTests
{
    private readonly IVagaRepository _repo = Substitute.For<IVagaRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly CadastrarVagaCommandHandler _sut;

    public CadastrarVagaCommandHandlerTests()
    {
        _repo.UnitOfWork.Returns(_uow);
        _uow.Commit().Returns(true);
        _sut = new CadastrarVagaCommandHandler(_repo);
    }
}
```

### Capturando argumentos com `Arg.Do<T>`

Para verificar o estado de um objeto passado a um mock:

```csharp
Usuario? usuarioCriado = null;
await _repo.AddAsync(Arg.Do<Usuario>(u => usuarioCriado = u), Arg.Any<CancellationToken>());

await _sut.Handle(command);

Assert.NotNull(usuarioCriado);
Assert.NotEqual("SenhaPlana", usuarioCriado!.SenhaHash);
```

### Testes parametrizados com `[Theory]`

```csharp
[Theory]
[InlineData(TipoVaga.Carro)]
[InlineData(TipoVaga.Moto)]
[InlineData(TipoVaga.Caminhonete)]
[InlineData(TipoVaga.DeficienteOuIdoso)]
public async Task Handle_TodosTiposDeVaga_Aceitos(TipoVaga tipo)
{
    _repo.GetByNumeroAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Vaga?)null);
    var result = await _sut.Handle(new CadastrarVagaCommand("X1", tipo));
    Assert.True(result.IsValid);
}
```

---

## Testes de Integração

### Como rodar

```bash
# Requer PostgreSQL rodando
docker compose up -d

dotnet test tests/ParkingSystem.Tests.IntegrationTests
```

### Como funciona

O projeto usa `WebApplicationFactory<Program>` do ASP.NET Core para subir a API completa em memória, apontando para um banco de testes separado (`parking_system_integration_test_db`).

```
ParkingWebAppFactory
    ├── ConfigureWebHost → injeta connection string de teste + JWT secret de teste
    ├── InitializeAsync  → EnsureCreated() nos dois DbContexts
    └── DisposeAsync     → EnsureDeleted() nos dois DbContexts (banco limpo após cada suite)
```

O banco `parking_system_integration_test_db` é **criado e destruído a cada execução** — cada run começa do zero.

### `ParkingWebAppFactory`

```csharp
public class ParkingWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestConnectionString,
                ["Jwt:Secret"] = "integration-test-secret-key-minimum-32-chars!"
            });
        });
    }
}
```

### `ProgramTestExport.cs`

Para que `WebApplicationFactory<Program>` funcione com top-level statements, o projeto WebApi contém:

```csharp
// ProgramTestExport.cs
public partial class Program { }
```

Isso expõe a classe `Program` gerada automaticamente pelo compilador.

### Estrutura dos testes de integração

```csharp
[Collection("IntegrationTests")]
public class AuthEndpointsTests(ParkingWebAppFactory factory) : IClassFixture<ParkingWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Registro_DadosValidos_Retorna201()
    {
        var request = new { nome = "Maria", email = $"maria_{Guid.NewGuid():N}@test.com", senha = "Senha@123" };

        var response = await _client.PostAsJsonAsync("/api/auth/registro", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

**Convenções:**
- E-mails com `Guid.NewGuid()` para evitar conflitos entre testes
- `IClassFixture<ParkingWebAppFactory>` compartilha a factory entre testes da mesma classe
- Cada classe de test recebe seu próprio `HttpClient`

---

## Cobertura de Testes

### Unitários (73 testes)

| Arquivo | Testes | O que cobre |
|---|---|---|
| `TarifaServiceTests` | 5 | Carência, arredondamento, teto diário |
| `VagaTests` | 14 | Criação, Ocupar, Liberar, Reservar, Manutenção |
| `ReservaTests` | 16 | Criação, Confirmar, Cancelar, Expirar, Utilizar |
| `UsuarioTests` | 11 | Criação, normalização de email, AtualizarSenha, roles |
| `RegistrarUsuarioCommandHandlerTests` | 6 | Validação, email duplicado, hash de senha |
| `CadastrarVagaCommandHandlerTests` | 7 | Validação, número duplicado, todos os tipos |
| `RegistrarEntradaCommandHandlerTests` | 8 | Validação, vaga não encontrada, placa em aberto |
| `RegistrarSaidaCommandHandlerTests` | 7 | Validação, saída, liberação de vaga, valor retornado |

### Integração (8 testes)

| Cenário | Endpoint |
|---|---|
| Registro com dados válidos | `POST /api/auth/registro` |
| Registro com email duplicado | `POST /api/auth/registro` |
| Registro com senha curta | `POST /api/auth/registro` |
| Login com credenciais válidas | `POST /api/auth/login` |
| Login com senha errada | `POST /api/auth/login` |
| Login com usuário inexistente | `POST /api/auth/login` |
| Endpoint protegido sem token | `GET /api/vagas` |
| Endpoint protegido com token válido | `GET /api/vagas` |
