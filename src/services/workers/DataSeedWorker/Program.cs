using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ParkingSystem.Module.Identity.Domain.Entities;
using ParkingSystem.Module.Identity.Domain.Enums;
using ParkingSystem.Module.Identity.Infra.Data.EF;
using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Module.Parking.Infra.Data.EF;
using ParkingSystem.Shared.Core.Services;

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
    var system = new SystemTenantProvider();
    var identityOptions = new DbContextOptionsBuilder<IdentityDbContext>().UseNpgsql(connectionString).Options;
    var parkingOptions  = new DbContextOptionsBuilder<ParkingDbContext>().UseNpgsql(connectionString).Options;

    await using var identityCtx = new IdentityDbContext(identityOptions, system);
    await using var parkingCtx  = new ParkingDbContext(parkingOptions, system);

    await SeedSuperAdminAsync(identityCtx, adminEmail, adminSenha, adminNome);
    var tenantId = await SeedDemoTenantAsync(identityCtx);
    await SeedTarifasAsync(parkingCtx, tenantId);
    await SeedVagasAsync(parkingCtx, tenantId);
    await SeedTestDataAsync(identityCtx, parkingCtx, tenantId);

    Console.WriteLine("Seed concluído.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"ERRO no seed: {ex.Message}");
    return 1;
}

static async Task SeedSuperAdminAsync(IdentityDbContext ctx, string email, string senha, string nome)
{
    var existe = await ctx.Usuarios.AnyAsync(u => u.Email == email.ToLowerInvariant());
    if (existe) { Console.WriteLine("[SuperAdmin] Já existe, pulando."); return; }

    var hasher  = new PasswordHasher<Usuario>();
    var hash    = hasher.HashPassword(null!, senha);
    var usuario = new Usuario(nome, email, hash, Role.SuperAdmin);

    ctx.Usuarios.Add(usuario);
    await ctx.Commit();
    Console.WriteLine($"[SuperAdmin] Criado: {email}");
}

static async Task<long> SeedDemoTenantAsync(IdentityDbContext ctx)
{
    var existing = await ctx.Tenants.FirstOrDefaultAsync(t => t.Nome == "Estacionamento Demo");
    if (existing is not null) { Console.WriteLine("[Tenant] Demo já existe, pulando."); return existing.Id; }

    var tenant = new Tenant("Estacionamento Demo", "DEMO0001");
    ctx.Tenants.Add(tenant);
    await ctx.Commit();
    Console.WriteLine($"[Tenant] Demo criado com código DEMO0001, id={tenant.Id}");
    return tenant.Id;
}

static async Task SeedTarifasAsync(ParkingDbContext ctx, long tenantId)
{
    if (await ctx.Tarifas.AnyAsync()) { Console.WriteLine("[Tarifas] Já existem, pulando."); return; }

    var tarifas = new[]
    {
        new Tarifa(tenantId, TipoVaga.Carro,             valorHora: 8m,  valorDiaria: 50m,  valorMensal: 600m),
        new Tarifa(tenantId, TipoVaga.Moto,              valorHora: 4m,  valorDiaria: 25m,  valorMensal: 300m),
        new Tarifa(tenantId, TipoVaga.Caminhonete,       valorHora: 12m, valorDiaria: 70m,  valorMensal: 800m),
        new Tarifa(tenantId, TipoVaga.DeficienteOuIdoso, valorHora: 1m,  valorDiaria: 5m,   valorMensal: 50m),
    };

    ctx.Tarifas.AddRange(tarifas);
    await ctx.Commit();
    Console.WriteLine($"[Tarifas] {tarifas.Length} tarifas criadas.");
}

static async Task SeedVagasAsync(ParkingDbContext ctx, long tenantId)
{
    if (await ctx.Vagas.AnyAsync()) { Console.WriteLine("[Vagas] Já existem, pulando."); return; }

    var vagas = new[]
    {
        new Vaga(tenantId, "A1", TipoVaga.Carro), new Vaga(tenantId, "A2", TipoVaga.Carro),
        new Vaga(tenantId, "A3", TipoVaga.Carro), new Vaga(tenantId, "A4", TipoVaga.Carro),
        new Vaga(tenantId, "B1", TipoVaga.Moto),  new Vaga(tenantId, "B2", TipoVaga.Moto),
        new Vaga(tenantId, "C1", TipoVaga.Caminhonete),
        new Vaga(tenantId, "D1", TipoVaga.DeficienteOuIdoso), new Vaga(tenantId, "D2", TipoVaga.DeficienteOuIdoso),
    };

    ctx.Vagas.AddRange(vagas);
    await ctx.Commit();
    Console.WriteLine($"[Vagas] {vagas.Length} vagas criadas.");
}

static async Task SeedTestDataAsync(IdentityDbContext identityCtx, ParkingDbContext parkingCtx, long tenantId)
{
    if (await identityCtx.Usuarios.AnyAsync(u => u.Email == "op1@parking.com"))
    {
        Console.WriteLine("[TestData] Já existe, pulando.");
        return;
    }

    var hasher = new PasswordHasher<Usuario>();
    var hash   = hasher.HashPassword(null!, "Senha@123");

    var testUsuarios = new[]
    {
        new Usuario("Carlos Operador",    "op1@parking.com",  hash, Role.Operador, tenantId),
        new Usuario("Fernanda Operador",  "op2@parking.com",  hash, Role.Operador, tenantId),
        new Usuario("João Silva",         "joao@teste.com",   hash, Role.Cliente,  tenantId),
        new Usuario("Maria Santos",       "maria@teste.com",  hash, Role.Cliente,  tenantId),
        new Usuario("Carlos Oliveira",    "carlos@teste.com", hash, Role.Cliente,  tenantId),
        new Usuario("Ana Costa",          "ana@teste.com",    hash, Role.Cliente,  tenantId),
        new Usuario("Pedro Lima",         "pedro@teste.com",  hash, Role.Cliente,  tenantId),
    };
    identityCtx.Usuarios.AddRange(testUsuarios);
    await identityCtx.Commit();
    Console.WriteLine($"[TestData] {testUsuarios.Length} usuários criados.");

    var joaoId   = (await identityCtx.Usuarios.FirstAsync(u => u.Email == "joao@teste.com")).Id;
    var mariaId  = (await identityCtx.Usuarios.FirstAsync(u => u.Email == "maria@teste.com")).Id;
    var carlosId = (await identityCtx.Usuarios.FirstAsync(u => u.Email == "carlos@teste.com")).Id;
    var anaId    = (await identityCtx.Usuarios.FirstAsync(u => u.Email == "ana@teste.com")).Id;
    var pedroId  = (await identityCtx.Usuarios.FirstAsync(u => u.Email == "pedro@teste.com")).Id;

    var vagas = await parkingCtx.Vagas.IgnoreQueryFilters().ToDictionaryAsync(v => v.Numero, v => v.Id);

    long A1 = vagas["A1"], A2 = vagas["A2"], A3 = vagas["A3"], A4 = vagas["A4"];
    long B1 = vagas["B1"], B2 = vagas["B2"];
    long C1 = vagas["C1"];
    long D1 = vagas["D1"], D2 = vagas["D2"];

    await parkingCtx.Database.ExecuteSqlRawAsync($"""
        INSERT INTO movimentacoes
            (tenant_id, vaga_id, placa_veiculo, data_entrada, data_saida, valor_total, pago, forma_pagamento, is_deleted, created_at, updated_at)
        VALUES
        ({tenantId},{A1},'ABC1234', NOW()-INTERVAL '1 day 16 hours',              NOW()-INTERVAL '1 day 14 hours',             16.00, true, 1, false, NOW()-INTERVAL '1 day 16 hours',              NOW()-INTERVAL '1 day 14 hours'),
        ({tenantId},{A2},'DEF5678', NOW()-INTERVAL '1 day 15 hours',              NOW()-INTERVAL '1 day 14 hours 48 minutes',   0.00, true, 0, false, NOW()-INTERVAL '1 day 15 hours',              NOW()-INTERVAL '1 day 14 hours 48 minutes'),
        ({tenantId},{B1},'GHI9012', NOW()-INTERVAL '1 day 10 hours',              NOW()-INTERVAL '1 day 7 hours 30 minutes',   12.00, true, 2, false, NOW()-INTERVAL '1 day 10 hours',              NOW()-INTERVAL '1 day 7 hours 30 minutes'),
        ({tenantId},{A3},'JKL3456', NOW()-INTERVAL '2 days 16 hours 30 minutes',  NOW()-INTERVAL '2 days 6 hours',             50.00, true, 1, false, NOW()-INTERVAL '2 days 16 hours 30 minutes',  NOW()-INTERVAL '2 days 6 hours'),
        ({tenantId},{C1},'MNO7890', NOW()-INTERVAL '2 days 16 hours',             NOW()-INTERVAL '2 days 11 hours',            60.00, true, 2, false, NOW()-INTERVAL '2 days 16 hours',             NOW()-INTERVAL '2 days 11 hours'),
        ({tenantId},{A4},'PQR2345', NOW()-INTERVAL '2 days 15 hours',             NOW()-INTERVAL '2 days 13 hours',            16.00, true, 0, false, NOW()-INTERVAL '2 days 15 hours',             NOW()-INTERVAL '2 days 13 hours'),
        ({tenantId},{B2},'STU6789', NOW()-INTERVAL '3 days 17 hours',             NOW()-INTERVAL '3 days 16 hours 15 minutes',  4.00, true, 0, false, NOW()-INTERVAL '3 days 17 hours',             NOW()-INTERVAL '3 days 16 hours 15 minutes'),
        ({tenantId},{A1},'VWX0123', NOW()-INTERVAL '3 days 16 hours',             NOW()-INTERVAL '3 days 6 hours 30 minutes',  50.00, true, 1, false, NOW()-INTERVAL '3 days 16 hours',             NOW()-INTERVAL '3 days 6 hours 30 minutes'),
        ({tenantId},{D1},'ABC5555', NOW()-INTERVAL '3 days 14 hours',             NOW()-INTERVAL '3 days 12 hours 30 minutes',  2.00, true, 2, false, NOW()-INTERVAL '3 days 14 hours',             NOW()-INTERVAL '3 days 12 hours 30 minutes'),
        ({tenantId},{A2},'DEF6666', NOW()-INTERVAL '4 days 17 hours',             NOW()-INTERVAL '4 days 5 hours',             50.00, true, 1, false, NOW()-INTERVAL '4 days 17 hours',             NOW()-INTERVAL '4 days 5 hours'),
        ({tenantId},{B1},'GHI7777', NOW()-INTERVAL '4 days 16 hours',             NOW()-INTERVAL '4 days 14 hours',             8.00, true, 0, false, NOW()-INTERVAL '4 days 16 hours',             NOW()-INTERVAL '4 days 14 hours'),
        ({tenantId},{A3},'JKL8888', NOW()-INTERVAL '5 days 15 hours',             NOW()-INTERVAL '5 days 14 hours 52 minutes',  0.00, true, 0, false, NOW()-INTERVAL '5 days 15 hours',             NOW()-INTERVAL '5 days 14 hours 52 minutes'),
        ({tenantId},{C1},'MNO9999', NOW()-INTERVAL '5 days 13 hours',             NOW()-INTERVAL '5 days 10 hours',            36.00, true, 2, false, NOW()-INTERVAL '5 days 13 hours',             NOW()-INTERVAL '5 days 10 hours'),
        ({tenantId},{A4},'PQR1111', NOW()-INTERVAL '6 days 16 hours',             NOW()-INTERVAL '6 days 4 hours',             50.00, true, 1, false, NOW()-INTERVAL '6 days 16 hours',             NOW()-INTERVAL '6 days 4 hours'),
        ({tenantId},{B2},'STU2222', NOW()-INTERVAL '6 days 16 hours 30 minutes',  NOW()-INTERVAL '6 days 15 hours',             8.00, true, 2, false, NOW()-INTERVAL '6 days 16 hours 30 minutes',  NOW()-INTERVAL '6 days 15 hours'),
        ({tenantId},{D2},'VWX3333', NOW()-INTERVAL '7 days 15 hours',             NOW()-INTERVAL '7 days 13 hours',             2.00, true, 0, false, NOW()-INTERVAL '7 days 15 hours',             NOW()-INTERVAL '7 days 13 hours'),
        ({tenantId},{A1},'ABC4444', NOW()-INTERVAL '7 days 16 hours',             NOW()-INTERVAL '7 days 7 hours',             50.00, true, 1, false, NOW()-INTERVAL '7 days 16 hours',             NOW()-INTERVAL '7 days 7 hours')
    """);

    await parkingCtx.Database.ExecuteSqlRawAsync($"""
        INSERT INTO movimentacoes
            (tenant_id, vaga_id, placa_veiculo, data_entrada, data_saida, valor_total, pago, forma_pagamento, is_deleted, created_at, updated_at)
        VALUES
        ({tenantId},{A2},'RTY5555', NOW()-INTERVAL '5 hours',    NULL, NULL, false, NULL, false, NOW()-INTERVAL '5 hours',    NOW()-INTERVAL '5 hours'),
        ({tenantId},{B1},'UIO6666', NOW()-INTERVAL '2 hours',    NULL, NULL, false, NULL, false, NOW()-INTERVAL '2 hours',    NOW()-INTERVAL '2 hours'),
        ({tenantId},{A3},'WER7777', NOW()-INTERVAL '45 minutes', NULL, NULL, false, NULL, false, NOW()-INTERVAL '45 minutes', NOW()-INTERVAL '45 minutes')
    """);
    Console.WriteLine("[TestData] 17 movimentações históricas + 3 abertas criadas.");

    await parkingCtx.Database.ExecuteSqlRawAsync(
        $"UPDATE vagas SET status = 1, updated_at = NOW() WHERE id IN ({A2}, {B1}, {A3})");
    await parkingCtx.Database.ExecuteSqlRawAsync(
        $"UPDATE vagas SET status = 3, updated_at = NOW() WHERE id = {D2}");

    await parkingCtx.Database.ExecuteSqlRawAsync($"""
        INSERT INTO reservas
            (tenant_id, vaga_id, usuario_id, data_agendada, data_limite, status, is_deleted, created_at, updated_at)
        VALUES
        ({tenantId},{A4},'{joaoId}',   NOW()-INTERVAL '3 days 10 hours', NOW()-INTERVAL '3 days 9 hours',  2, false, NOW()-INTERVAL '3 days 11 hours',  NOW()-INTERVAL '3 days 10 hours 30 minutes'),
        ({tenantId},{A1},'{mariaId}',  NOW()-INTERVAL '4 days 15 hours', NOW()-INTERVAL '4 days 14 hours', 3, false, NOW()-INTERVAL '4 days 16 hours',  NOW()-INTERVAL '4 days 14 hours'),
        ({tenantId},{B2},'{carlosId}', NOW()-INTERVAL '5 days 14 hours', NOW()-INTERVAL '5 days 13 hours', 4, false, NOW()-INTERVAL '5 days 15 hours',  NOW()-INTERVAL '5 days 13 hours'),
        ({tenantId},{A2},'{anaId}',    NOW()-INTERVAL '2 days 13 hours', NOW()-INTERVAL '2 days 12 hours', 2, false, NOW()-INTERVAL '2 days 14 hours',  NOW()-INTERVAL '2 days 13 hours 30 minutes'),
        ({tenantId},{C1},'{pedroId}',  NOW()-INTERVAL '1 day 8 hours',   NOW()-INTERVAL '1 day 7 hours',  3, false, NOW()-INTERVAL '1 day 9 hours',    NOW()-INTERVAL '1 day 8 hours')
    """);

    await parkingCtx.Database.ExecuteSqlRawAsync($"""
        INSERT INTO reservas
            (tenant_id, vaga_id, usuario_id, data_agendada, data_limite, status, is_deleted, created_at, updated_at)
        VALUES
        ({tenantId},{A4},'{joaoId}',   NOW()+INTERVAL '1 day 1 hour',   NOW()+INTERVAL '1 day 2 hours',  0, false, NOW(), NOW()),
        ({tenantId},{B2},'{mariaId}',  NOW()+INTERVAL '1 day 6 hours',  NOW()+INTERVAL '1 day 7 hours',  1, false, NOW(), NOW()),
        ({tenantId},{D1},'{carlosId}', NOW()+INTERVAL '2 days 2 hours', NOW()+INTERVAL '2 days 3 hours', 0, false, NOW(), NOW())
    """);
    Console.WriteLine("[TestData] 5 reservas históricas + 3 futuras criadas.");

    await parkingCtx.Database.ExecuteSqlRawAsync(
        $"UPDATE vagas SET status = 2, updated_at = NOW() WHERE id = {B2}");

    Console.WriteLine("[TestData] Seed de teste concluído.");
    Console.WriteLine("  Logins de teste: op1@parking.com, op2@parking.com (Senha@123)");
    Console.WriteLine("  Clientes: joao@teste.com, maria@teste.com, carlos@teste.com, ana@teste.com, pedro@teste.com (Senha@123)");
}
