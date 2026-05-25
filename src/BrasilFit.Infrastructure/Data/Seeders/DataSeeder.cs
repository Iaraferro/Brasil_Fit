using BrasilFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BrasilFit.Infrastructure.Data.Seeders;

// Popular dados iniciais minimos: 1 Administrador padrao e alguns Alimentos.
// O hashing da senha e injetado como delegate para nao acoplar Infrastructure ao BCrypt.
public class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataSeeder> _logger;
    private readonly Func<string, string> _hashSenha;

    public DataSeeder(
        AppDbContext context,
        IConfiguration configuration,
        ILogger<DataSeeder> logger,
        Func<string, string> hashSenha)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _hashSenha = hashSenha;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Garante que o banco esta criado/migrado antes de popular.
        await _context.Database.MigrateAsync(ct);

        await SeedAdministradorAsync(ct);
        await SeedAlimentosAsync(ct);

        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedAdministradorAsync(CancellationToken ct)
    {
        var emailAdmin = _configuration["Seed:Admin:Email"] ?? "admin@brasilfit.local";
        var senhaAdmin = _configuration["Seed:Admin:Senha"] ?? "Admin@123";
        var nomeAdmin = _configuration["Seed:Admin:Nome"] ?? "Administrador Padrao";

        var jaExiste = await _context.Administradores.AnyAsync(a => a.Email == emailAdmin, ct);
        if (jaExiste)
        {
            _logger.LogInformation("Administrador padrao ja existe ({Email}).", emailAdmin);
            return;
        }

        var admin = new Administrador
        {
            Nome = nomeAdmin,
            Email = emailAdmin,
            SenhaHash = _hashSenha(senhaAdmin),
            Cargo = "Administrador do Sistema",
            Ativo = true
        };

        _context.Administradores.Add(admin);
        _logger.LogInformation("Administrador padrao criado: {Email}.", emailAdmin);
    }

    private async Task SeedAlimentosAsync(CancellationToken ct)
    {
        if (await _context.Alimentos.AnyAsync(ct))
        {
            return;
        }

        // Valores nutricionais aproximados por 100g (referencia: TACO / OpenFoodFacts).
        var alimentos = new[]
        {
            new Alimento { Nome = "Arroz branco cozido",   Kcal = 128, Carboidratos = 28.1m, Proteinas = 2.5m, Lipidios = 0.2m },
            new Alimento { Nome = "Feijao carioca cozido", Kcal = 76,  Carboidratos = 13.6m, Proteinas = 4.8m, Lipidios = 0.5m },
            new Alimento { Nome = "Peito de frango grelhado", Kcal = 159, Carboidratos = 0m, Proteinas = 32m,  Lipidios = 3.0m },
            new Alimento { Nome = "Ovo de galinha cozido",  Kcal = 146, Carboidratos = 0.6m, Proteinas = 13.3m, Lipidios = 9.5m },
            new Alimento { Nome = "Banana prata",           Kcal = 89,  Carboidratos = 23.8m, Proteinas = 1.3m, Lipidios = 0.1m },
            new Alimento { Nome = "Maca",                   Kcal = 56,  Carboidratos = 15.2m, Proteinas = 0.3m, Lipidios = 0m   },
            new Alimento { Nome = "Pao frances",            Kcal = 300, Carboidratos = 58.6m, Proteinas = 8.0m, Lipidios = 3.1m },
            new Alimento { Nome = "Leite integral",         Kcal = 61,  Carboidratos = 4.3m,  Proteinas = 2.9m, Lipidios = 3.5m },
            new Alimento { Nome = "Batata doce cozida",     Kcal = 77,  Carboidratos = 18.4m, Proteinas = 0.6m, Lipidios = 0.1m },
            new Alimento { Nome = "Aveia em flocos",        Kcal = 394, Carboidratos = 66.6m, Proteinas = 13.9m, Lipidios = 8.5m }
        };

        await _context.Alimentos.AddRangeAsync(alimentos, ct);
        _logger.LogInformation("Seed: {Quantidade} alimentos basicos inseridos.", alimentos.Length);
    }
}
