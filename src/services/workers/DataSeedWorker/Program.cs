using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ParkingSystem.Module.Identity.Domain.Entities;
using ParkingSystem.Module.Identity.Domain.Enums;
using ParkingSystem.Module.Identity.Infra.Data.EF;
using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Module.Parking.Infra.Data.EF;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{env}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = config.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("ERRO: ConnectionStrings__DefaultConnection não configurada.");
    return 1;
}

var adminEmail = config["Seed:AdminEmail"] ?? "admin@parking.com";
var adminSenha = config["Seed:AdminSenha"];
var adminNome  = config["Seed:AdminNome"] ?? "Administrador";

if (string.IsNullOrWhiteSpace(adminSenha))
{
    Console.Error.WriteLine("ERRO: Seed__AdminSenha não configurada.");
    return 1;
}

Console.WriteLine("Iniciando seed de dados...");

try
{
    var identityOptions = new DbContextOptionsBuilder<IdentityDbContext>().UseNpgsql(connectionString).Options;
    var parkingOptions  = new DbContextOptionsBuilder<ParkingDbContext>().UseNpgsql(connectionString).Options;

    await using var identityCtx = new IdentityDbContext(identityOptions);
    await using var parkingCtx  = new ParkingDbContext(parkingOptions);

    await SeedAdminAsync(identityCtx, adminEmail, adminSenha, adminNome);
    await SeedTarifasAsync(parkingCtx);
    await SeedVagasAsync(parkingCtx);
    await SeedTestDataAsync(identityCtx, parkingCtx);

    Console.WriteLine("Seed concluído.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"ERRO no seed: {ex.Message}");
    return 1;
}

static async Task SeedAdminAsync(IdentityDbContext ctx, string email, string senha, string nome)
{
    var existe = await ctx.Usuarios.AnyAsync(u => u.Email == email.ToLowerInvariant());
    if (existe) { Console.WriteLine("[Admin] Já existe, pulando."); return; }

    var hasher  = new PasswordHasher<Usuario>();
    var hash    = hasher.HashPassword(null!, senha);
    var usuario = new Usuario(nome, email, hash, Role.Admin);

    ctx.Usuarios.Add(usuario);
    await ctx.Commit();
    Console.WriteLine($"[Admin] Criado: {email}");
}

static async Task SeedTarifasAsync(ParkingDbContext ctx)
{
    if (await ctx.Tarifas.AnyAsync()) { Console.WriteLine("[Tarifas] Já existem, pulando."); return; }

    var tarifas = new[]
    {
        new Tarifa(TipoVaga.Carro,              valorHora: 8m,  valorDiaria: 50m,  valorMensal: 600m),
        new Tarifa(TipoVaga.Moto,               valorHora: 4m,  valorDiaria: 25m,  valorMensal: 300m),
        new Tarifa(TipoVaga.Caminhonete,        valorHora: 12m, valorDiaria: 70m,  valorMensal: 800m),
        new Tarifa(TipoVaga.DeficienteOuIdoso,  valorHora: 1m,  valorDiaria: 5m,   valorMensal: 50m),
    };

    ctx.Tarifas.AddRange(tarifas);
    await ctx.Commit();
    Console.WriteLine($"[Tarifas] {tarifas.Length} tarifas criadas.");
}

static async Task SeedVagasAsync(ParkingDbContext ctx)
{
    if (await ctx.Vagas.AnyAsync()) { Console.WriteLine("[Vagas] Já existem, pulando."); return; }

    var vagas = new[]
    {
        new Vaga("A1", TipoVaga.Carro), new Vaga("A2", TipoVaga.Carro),
        new Vaga("A3", TipoVaga.Carro), new Vaga("A4", TipoVaga.Carro),
        new Vaga("B1", TipoVaga.Moto),  new Vaga("B2", TipoVaga.Moto),
        new Vaga("C1", TipoVaga.Caminhonete),
        new Vaga("D1", TipoVaga.DeficienteOuIdoso), new Vaga("D2", TipoVaga.DeficienteOuIdoso),
    };

    ctx.Vagas.AddRange(vagas);
    await ctx.Commit();
    Console.WriteLine($"[Vagas] {vagas.Length} vagas criadas.");
}

static async Task SeedTestDataAsync(IdentityDbContext identityCtx, ParkingDbContext parkingCtx)
{
    if (await identityCtx.Usuarios.AnyAsync(u => u.Email == "op1@parking.com"))
    {
        Console.WriteLine("[TestData] Já existe, pulando.");
        return;
    }

    // 1. Usuários de teste
    var hasher = new PasswordHasher<Usuario>();
    var hash   = hasher.HashPassword(null!, "Senha@123");

    var testUsuarios = new[]
    {
        new Usuario("Carlos Operador",    "op1@parking.com",  hash, Role.Operador),
        new Usuario("Fernanda Operador",  "op2@parking.com",  hash, Role.Operador),
        new Usuario("João Silva",         "joao@teste.com",   hash, Role.Cliente),
        new Usuario("Maria Santos",       "maria@teste.com",  hash, Role.Cliente),
        new Usuario("Carlos Oliveira",    "carlos@teste.com", hash, Role.Cliente),
        new Usuario("Ana Costa",          "ana@teste.com",    hash, Role.Cliente),
        new Usuario("Pedro Lima",         "pedro@teste.com",  hash, Role.Cliente),
    };
    identityCtx.Usuarios.AddRange(testUsuarios);
    await identityCtx.Commit();
    Console.WriteLine($"[TestData] {testUsuarios.Length} usuários criados.");

    // 2. IDs dos clientes (usados nas reservas)
    var joaoId   = (await identityCtx.Usuarios.FirstAsync(u => u.Email == "joao@teste.com")).Id;
    var mariaId  = (await identityCtx.Usuarios.FirstAsync(u => u.Email == "maria@teste.com")).Id;
    var carlosId = (await identityCtx.Usuarios.FirstAsync(u => u.Email == "carlos@teste.com")).Id;
    var anaId    = (await identityCtx.Usuarios.FirstAsync(u => u.Email == "ana@teste.com")).Id;
    var pedroId  = (await identityCtx.Usuarios.FirstAsync(u => u.Email == "pedro@teste.com")).Id;

    // 3. IDs das vagas
    var vagas = await parkingCtx.Vagas.IgnoreQueryFilters().ToDictionaryAsync(v => v.Numero, v => v.Id);

    long A1 = vagas["A1"], A2 = vagas["A2"], A3 = vagas["A3"], A4 = vagas["A4"];
    long B1 = vagas["B1"], B2 = vagas["B2"];
    long C1 = vagas["C1"];
    long D1 = vagas["D1"], D2 = vagas["D2"];

    // 4. Movimentações históricas (passadas, concluídas e pagas)
    // FormaPagamento: 0=Dinheiro, 1=Cartao, 2=Pix
    // Tarifa: Carro 8/h cap50 | Moto 4/h cap25 | Caminhonete 12/h cap70 | Deficiente 1/h cap5
    await parkingCtx.Database.ExecuteSqlRawAsync($"""
        INSERT INTO movimentacoes
            (vaga_id, placa_veiculo, data_entrada, data_saida, valor_total, pago, forma_pagamento, is_deleted, created_at, updated_at)
        VALUES
        -- D-1: ABC1234 em A1 (Carro 2h → 16), DEF5678 em A2 (Carro 12min → grátis), GHI9012 em B1 (Moto 2.5h→3h → 12)
        ({A1},'ABC1234', NOW()-INTERVAL '1 day 16 hours', NOW()-INTERVAL '1 day 14 hours',     16.00, true, 1, false, NOW()-INTERVAL '1 day 16 hours', NOW()-INTERVAL '1 day 14 hours'),
        ({A2},'DEF5678', NOW()-INTERVAL '1 day 15 hours', NOW()-INTERVAL '1 day 14 hours 48 minutes', 0.00, true, 0, false, NOW()-INTERVAL '1 day 15 hours', NOW()-INTERVAL '1 day 14 hours 48 minutes'),
        ({B1},'GHI9012', NOW()-INTERVAL '1 day 10 hours', NOW()-INTERVAL '1 day 7 hours 30 minutes',  12.00, true, 2, false, NOW()-INTERVAL '1 day 10 hours', NOW()-INTERVAL '1 day 7 hours 30 minutes'),
        -- D-2: JKL3456 em A3 (Carro 10.5h→cap 50), MNO7890 em C1 (Cami 5h→60), PQR2345 em A4 (Carro 2h→16)
        ({A3},'JKL3456', NOW()-INTERVAL '2 days 16 hours 30 minutes', NOW()-INTERVAL '2 days 6 hours',         50.00, true, 1, false, NOW()-INTERVAL '2 days 16 hours 30 minutes', NOW()-INTERVAL '2 days 6 hours'),
        ({C1},'MNO7890', NOW()-INTERVAL '2 days 16 hours',            NOW()-INTERVAL '2 days 11 hours',        60.00, true, 2, false, NOW()-INTERVAL '2 days 16 hours', NOW()-INTERVAL '2 days 11 hours'),
        ({A4},'PQR2345', NOW()-INTERVAL '2 days 15 hours',            NOW()-INTERVAL '2 days 13 hours',        16.00, true, 0, false, NOW()-INTERVAL '2 days 15 hours', NOW()-INTERVAL '2 days 13 hours'),
        -- D-3: STU6789 em B2 (Moto 45min→4), VWX0123 em A1 (Carro 9.5h→cap 50), ABC5555 em D1 (Defi 1.5h→2)
        ({B2},'STU6789', NOW()-INTERVAL '3 days 17 hours',            NOW()-INTERVAL '3 days 16 hours 15 minutes', 4.00, true, 0, false, NOW()-INTERVAL '3 days 17 hours', NOW()-INTERVAL '3 days 16 hours 15 minutes'),
        ({A1},'VWX0123', NOW()-INTERVAL '3 days 16 hours',            NOW()-INTERVAL '3 days 6 hours 30 minutes',  50.00, true, 1, false, NOW()-INTERVAL '3 days 16 hours', NOW()-INTERVAL '3 days 6 hours 30 minutes'),
        ({D1},'ABC5555', NOW()-INTERVAL '3 days 14 hours',            NOW()-INTERVAL '3 days 12 hours 30 minutes', 2.00, true, 2, false, NOW()-INTERVAL '3 days 14 hours', NOW()-INTERVAL '3 days 12 hours 30 minutes'),
        -- D-4: DEF6666 em A2 (Carro 12h→cap 50), GHI7777 em B1 (Moto 2h→8)
        ({A2},'DEF6666', NOW()-INTERVAL '4 days 17 hours',            NOW()-INTERVAL '4 days 5 hours',             50.00, true, 1, false, NOW()-INTERVAL '4 days 17 hours', NOW()-INTERVAL '4 days 5 hours'),
        ({B1},'GHI7777', NOW()-INTERVAL '4 days 16 hours',            NOW()-INTERVAL '4 days 14 hours',            8.00,  true, 0, false, NOW()-INTERVAL '4 days 16 hours', NOW()-INTERVAL '4 days 14 hours'),
        -- D-5: JKL8888 em A3 (Carro 8min→grátis), MNO9999 em C1 (Cami 3h→36)
        ({A3},'JKL8888', NOW()-INTERVAL '5 days 15 hours',            NOW()-INTERVAL '5 days 14 hours 52 minutes', 0.00, true, 0, false, NOW()-INTERVAL '5 days 15 hours', NOW()-INTERVAL '5 days 14 hours 52 minutes'),
        ({C1},'MNO9999', NOW()-INTERVAL '5 days 13 hours',            NOW()-INTERVAL '5 days 10 hours',            36.00, true, 2, false, NOW()-INTERVAL '5 days 13 hours', NOW()-INTERVAL '5 days 10 hours'),
        -- D-6: PQR1111 em A4 (Carro 12h→cap 50), STU2222 em B2 (Moto 1.5h→8)
        ({A4},'PQR1111', NOW()-INTERVAL '6 days 16 hours',            NOW()-INTERVAL '6 days 4 hours',             50.00, true, 1, false, NOW()-INTERVAL '6 days 16 hours', NOW()-INTERVAL '6 days 4 hours'),
        ({B2},'STU2222', NOW()-INTERVAL '6 days 16 hours 30 minutes', NOW()-INTERVAL '6 days 15 hours',            8.00,  true, 2, false, NOW()-INTERVAL '6 days 16 hours 30 minutes', NOW()-INTERVAL '6 days 15 hours'),
        -- D-7: VWX3333 em D2 (Defi 2h→2), ABC4444 em A1 (Carro 9h→cap 50)
        ({D2},'VWX3333', NOW()-INTERVAL '7 days 15 hours',            NOW()-INTERVAL '7 days 13 hours',            2.00,  true, 0, false, NOW()-INTERVAL '7 days 15 hours', NOW()-INTERVAL '7 days 13 hours'),
        ({A1},'ABC4444', NOW()-INTERVAL '7 days 16 hours',            NOW()-INTERVAL '7 days 7 hours',             50.00, true, 1, false, NOW()-INTERVAL '7 days 16 hours', NOW()-INTERVAL '7 days 7 hours')
    """);

    // 5. Movimentações abertas (veículos atualmente no pátio)
    await parkingCtx.Database.ExecuteSqlRawAsync($"""
        INSERT INTO movimentacoes
            (vaga_id, placa_veiculo, data_entrada, data_saida, valor_total, pago, forma_pagamento, is_deleted, created_at, updated_at)
        VALUES
        ({A2},'RTY5555', NOW()-INTERVAL '5 hours',   NULL, NULL, false, NULL, false, NOW()-INTERVAL '5 hours',   NOW()-INTERVAL '5 hours'),
        ({B1},'UIO6666', NOW()-INTERVAL '2 hours',   NULL, NULL, false, NULL, false, NOW()-INTERVAL '2 hours',   NOW()-INTERVAL '2 hours'),
        ({A3},'WER7777', NOW()-INTERVAL '45 minutes',NULL, NULL, false, NULL, false, NOW()-INTERVAL '45 minutes',NOW()-INTERVAL '45 minutes')
    """);
    Console.WriteLine("[TestData] 17 movimentações históricas + 3 abertas criadas.");

    // 6. Atualizar status das vagas ocupadas e em manutenção
    // StatusVaga: 0=Disponivel, 1=Ocupada, 2=Reservada, 3=Manutencao
    await parkingCtx.Database.ExecuteSqlRawAsync(
        $"UPDATE vagas SET status = 1, updated_at = NOW() WHERE id IN ({A2}, {B1}, {A3})");
    await parkingCtx.Database.ExecuteSqlRawAsync(
        $"UPDATE vagas SET status = 3, updated_at = NOW() WHERE id = {D2}");

    // 7. Reservas passadas (via SQL para contornar validação de data futura do domínio)
    // StatusReserva: 0=Pendente, 1=Confirmada, 2=Cancelada, 3=Expirada, 4=Utilizada
    await parkingCtx.Database.ExecuteSqlRawAsync($"""
        INSERT INTO reservas
            (vaga_id, usuario_id, data_agendada, data_limite, status, is_deleted, created_at, updated_at)
        VALUES
        ({A4},'{joaoId}',   NOW()-INTERVAL '3 days 10 hours', NOW()-INTERVAL '3 days 9 hours',  2, false, NOW()-INTERVAL '3 days 11 hours', NOW()-INTERVAL '3 days 10 hours 30 minutes'),
        ({A1},'{mariaId}',  NOW()-INTERVAL '4 days 15 hours', NOW()-INTERVAL '4 days 14 hours', 3, false, NOW()-INTERVAL '4 days 16 hours', NOW()-INTERVAL '4 days 14 hours'),
        ({B2},'{carlosId}', NOW()-INTERVAL '5 days 14 hours', NOW()-INTERVAL '5 days 13 hours', 4, false, NOW()-INTERVAL '5 days 15 hours', NOW()-INTERVAL '5 days 13 hours'),
        ({A2},'{anaId}',    NOW()-INTERVAL '2 days 13 hours', NOW()-INTERVAL '2 days 12 hours', 2, false, NOW()-INTERVAL '2 days 14 hours', NOW()-INTERVAL '2 days 13 hours 30 minutes'),
        ({C1},'{pedroId}',  NOW()-INTERVAL '1 day 8 hours',   NOW()-INTERVAL '1 day 7 hours',  3, false, NOW()-INTERVAL '1 day 9 hours',   NOW()-INTERVAL '1 day 8 hours')
    """);

    // 8. Reservas futuras
    await parkingCtx.Database.ExecuteSqlRawAsync($"""
        INSERT INTO reservas
            (vaga_id, usuario_id, data_agendada, data_limite, status, is_deleted, created_at, updated_at)
        VALUES
        ({A4},'{joaoId}',   NOW()+INTERVAL '1 day 1 hour',   NOW()+INTERVAL '1 day 2 hours',  0, false, NOW(), NOW()),
        ({B2},'{mariaId}',  NOW()+INTERVAL '1 day 6 hours',  NOW()+INTERVAL '1 day 7 hours',  1, false, NOW(), NOW()),
        ({D1},'{carlosId}', NOW()+INTERVAL '2 days 2 hours', NOW()+INTERVAL '2 days 3 hours', 0, false, NOW(), NOW())
    """);
    Console.WriteLine("[TestData] 5 reservas históricas + 3 futuras criadas.");

    // 9. Vaga com reserva confirmada fica Reservada
    await parkingCtx.Database.ExecuteSqlRawAsync(
        $"UPDATE vagas SET status = 2, updated_at = NOW() WHERE id = {B2}");

    Console.WriteLine("[TestData] Seed de teste concluído.");
    Console.WriteLine("  Logins de teste: op1@parking.com, op2@parking.com (Senha@123)");
    Console.WriteLine("  Clientes: joao@teste.com, maria@teste.com, carlos@teste.com, ana@teste.com, pedro@teste.com (Senha@123)");
}
