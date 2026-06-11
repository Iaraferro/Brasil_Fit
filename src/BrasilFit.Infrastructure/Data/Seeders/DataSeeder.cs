using BrasilFit.Domain.Entities;
using BrasilFit.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BrasilFit.Infrastructure.Data.Seeders;

// Popular dados iniciais: 1 Administrador padrao, alimentos basicos e dados de demonstracao.
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
        await _context.Database.MigrateAsync(ct);

        await SeedAdministradorAsync(ct);
        await SeedAlimentosAsync(ct);
        await _context.SaveChangesAsync(ct); // Persiste admin e alimentos antes dos dados de demo

        await SeedDemoDataAsync(ct);
        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedAdministradorAsync(CancellationToken ct)
    {
        var emailAdmin = _configuration["Seed:Admin:Email"] ?? "admin@brasilfit.local";
        var senhaAdmin = _configuration["Seed:Admin:Senha"] ?? "Admin@123";
        var nomeAdmin  = _configuration["Seed:Admin:Nome"]  ?? "Administrador Padrao";

        var jaExiste = await _context.Administradores.AnyAsync(a => a.Email == emailAdmin, ct);
        if (jaExiste)
        {
            _logger.LogInformation("Administrador padrao ja existe ({Email}).", emailAdmin);
            return;
        }

        _context.Administradores.Add(new Administrador
        {
            Nome     = nomeAdmin,
            Email    = emailAdmin,
            SenhaHash = _hashSenha(senhaAdmin),
            Cargo    = "Administrador do Sistema",
            Ativo    = true
        });
        _logger.LogInformation("Administrador padrao criado: {Email}.", emailAdmin);
    }

    private async Task SeedAlimentosAsync(CancellationToken ct)
    {
        if (await _context.Alimentos.AnyAsync(ct)) return;

        var alimentos = new[]
        {
            new Alimento { Nome = "Arroz branco cozido",      Kcal = 128, Carboidratos = 28.1m, Proteinas = 2.5m,  Lipidios = 0.2m },
            new Alimento { Nome = "Feijao carioca cozido",    Kcal = 76,  Carboidratos = 13.6m, Proteinas = 4.8m,  Lipidios = 0.5m },
            new Alimento { Nome = "Peito de frango grelhado", Kcal = 159, Carboidratos = 0m,    Proteinas = 32m,   Lipidios = 3.0m },
            new Alimento { Nome = "Ovo de galinha cozido",    Kcal = 146, Carboidratos = 0.6m,  Proteinas = 13.3m, Lipidios = 9.5m },
            new Alimento { Nome = "Banana prata",             Kcal = 89,  Carboidratos = 23.8m, Proteinas = 1.3m,  Lipidios = 0.1m },
            new Alimento { Nome = "Maca",                     Kcal = 56,  Carboidratos = 15.2m, Proteinas = 0.3m,  Lipidios = 0m   },
            new Alimento { Nome = "Pao frances",              Kcal = 300, Carboidratos = 58.6m, Proteinas = 8.0m,  Lipidios = 3.1m },
            new Alimento { Nome = "Leite integral",           Kcal = 61,  Carboidratos = 4.3m,  Proteinas = 2.9m,  Lipidios = 3.5m },
            new Alimento { Nome = "Batata doce cozida",       Kcal = 77,  Carboidratos = 18.4m, Proteinas = 0.6m,  Lipidios = 0.1m },
            new Alimento { Nome = "Aveia em flocos",          Kcal = 394, Carboidratos = 66.6m, Proteinas = 13.9m, Lipidios = 8.5m }
        };

        await _context.Alimentos.AddRangeAsync(alimentos, ct);
        _logger.LogInformation("Seed: {Qtd} alimentos basicos inseridos.", alimentos.Length);
    }

    private async Task SeedDemoDataAsync(CancellationToken ct)
    {
        if (await _context.Nutricionistas.AnyAsync(ct))
        {
            _logger.LogInformation("Dados de demonstracao ja existem. Pulando seed.");
            return;
        }

        _logger.LogInformation("Inserindo dados de demonstracao...");

        // Carrega IDs dos alimentos ja persistidos (IDs 1-10)
        var alimentoIds = await _context.Alimentos.Select(a => a.Id).ToListAsync(ct);
        if (alimentoIds.Count < 10)
        {
            _logger.LogWarning("Alimentos insuficientes para seed de demo.");
            return;
        }
        int idArroz = alimentoIds[0], idFeijao = alimentoIds[1], idFrango = alimentoIds[2];
        int idOvo   = alimentoIds[3], idBanana = alimentoIds[4], idMaca   = alimentoIds[5];
        int idPao   = alimentoIds[6], idLeite  = alimentoIds[7], idBatata = alimentoIds[8];
        int idAveia = alimentoIds[9];

        // ── Nutricionista ──────────────────────────────────────────────────────
        var nutricionista = new Nutricionista
        {
            Nome        = "Dra. Ana Lima",
            Email       = "ana.lima@brasilfit.local",
            SenhaHash   = _hashSenha("Nutri@123"),
            Crn         = "12345/DF",
            Especialidade = "Nutricao Esportiva",
            Ativo       = true
        };
        _context.Nutricionistas.Add(nutricionista);

        // ── Pacientes ──────────────────────────────────────────────────────────
        var carlos = new Paciente
        {
            Nome            = "Carlos Souza",
            Email           = "carlos.souza@brasilfit.local",
            SenhaHash       = _hashSenha("Paciente@123"),
            DataNascimento  = new DateTime(1991, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            Sexo            = Sexo.Masculino,
            Telefone        = "(11) 98765-4321",
            HistoricoClinico = "Hipertensao leve controlada. Sedentario ate 2025.",
            Nutricionista   = nutricionista,
            Ativo           = true,
            Endereco = new Endereco
            {
                Cep        = "01310-100",
                Logradouro = "Rua das Acacias",
                Numero     = "123",
                Complemento = "Apto 45",
                Bairro     = "Centro",
                Cidade     = "Sao Paulo",
                Uf         = "SP"
            }
        };

        var mariana = new Paciente
        {
            Nome            = "Mariana Ferreira",
            Email           = "mariana.ferreira@brasilfit.local",
            SenhaHash       = _hashSenha("Paciente@123"),
            DataNascimento  = new DateTime(1998, 7, 22, 0, 0, 0, DateTimeKind.Utc),
            Sexo            = Sexo.Feminino,
            Telefone        = "(62) 91234-5678",
            HistoricoClinico = "Sem historico de doencas relevantes. Pratica musculacao 3x/semana.",
            Nutricionista   = nutricionista,
            Ativo           = true,
            Endereco = new Endereco
            {
                Cep        = "74000-000",
                Logradouro = "Av. Brasil",
                Numero     = "456",
                Bairro     = "Jardim America",
                Cidade     = "Goiania",
                Uf         = "GO"
            }
        };

        _context.Pacientes.Add(carlos);
        _context.Pacientes.Add(mariana);

        // ── Plano Alimentar — Carlos ───────────────────────────────────────────
        var planoCarlos = new PlanoAlimentar
        {
            Nome         = "Emagrecimento Saudavel - Carlos",
            Objetivo     = "Reducao de peso corporal e gordura visceral",
            DataInicio   = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            DuracaoDias  = 90,
            Observacoes  = "Evitar frituras e ultraprocessados. Ingestao de 2,5L de agua/dia.",
            Ativo        = true,
            Paciente     = carlos,
            Nutricionista = nutricionista,
            Refeicoes = new List<Refeicao>
            {
                new Refeicao
                {
                    Tipo      = TipoRefeicao.CafeDaManha,
                    Horario   = new TimeOnly(7, 0),
                    Observacoes = "Preferir aveia com frutas",
                    Itens = new List<ItemRefeicao>
                    {
                        new ItemRefeicao { AlimentoId = idAveia,  Quantidade = 50,  Unidade = "g"  },
                        new ItemRefeicao { AlimentoId = idLeite,  Quantidade = 200, Unidade = "ml" },
                        new ItemRefeicao { AlimentoId = idBanana, Quantidade = 1,   Unidade = "un" }
                    }
                },
                new Refeicao
                {
                    Tipo    = TipoRefeicao.LancheDaManha,
                    Horario = new TimeOnly(10, 0),
                    Itens = new List<ItemRefeicao>
                    {
                        new ItemRefeicao { AlimentoId = idMaca, Quantidade = 1, Unidade = "un" }
                    }
                },
                new Refeicao
                {
                    Tipo      = TipoRefeicao.Almoco,
                    Horario   = new TimeOnly(12, 30),
                    Observacoes = "Prato colorido com legumes variados",
                    Itens = new List<ItemRefeicao>
                    {
                        new ItemRefeicao { AlimentoId = idArroz,  Quantidade = 150, Unidade = "g" },
                        new ItemRefeicao { AlimentoId = idFeijao, Quantidade = 100, Unidade = "g" },
                        new ItemRefeicao { AlimentoId = idFrango, Quantidade = 150, Unidade = "g" },
                        new ItemRefeicao { AlimentoId = idBatata, Quantidade = 100, Unidade = "g" }
                    }
                },
                new Refeicao
                {
                    Tipo    = TipoRefeicao.LancheDaTarde,
                    Horario = new TimeOnly(15, 30),
                    Itens = new List<ItemRefeicao>
                    {
                        new ItemRefeicao { AlimentoId = idOvo, Quantidade = 2, Unidade = "un" }
                    }
                },
                new Refeicao
                {
                    Tipo      = TipoRefeicao.Jantar,
                    Horario   = new TimeOnly(19, 0),
                    Observacoes = "Refeicao leve",
                    Itens = new List<ItemRefeicao>
                    {
                        new ItemRefeicao { AlimentoId = idFrango, Quantidade = 120, Unidade = "g" },
                        new ItemRefeicao { AlimentoId = idBatata, Quantidade = 100, Unidade = "g" }
                    }
                }
            }
        };

        // ── Plano Alimentar — Mariana ──────────────────────────────────────────
        var planoMariana = new PlanoAlimentar
        {
            Nome         = "Hipertrofia e Ganho de Massa - Mariana",
            Objetivo     = "Aumento de massa muscular com baixo ganho de gordura",
            DataInicio   = new DateTime(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc),
            DuracaoDias  = 60,
            Observacoes  = "Ingestao proteica de 1,8g/kg. Carboidratos concentrados ao redor do treino.",
            Ativo        = true,
            Paciente     = mariana,
            Nutricionista = nutricionista,
            Refeicoes = new List<Refeicao>
            {
                new Refeicao
                {
                    Tipo      = TipoRefeicao.CafeDaManha,
                    Horario   = new TimeOnly(6, 30),
                    Observacoes = "Pre-treino ou logo apos acordar",
                    Itens = new List<ItemRefeicao>
                    {
                        new ItemRefeicao { AlimentoId = idAveia,  Quantidade = 80,  Unidade = "g"  },
                        new ItemRefeicao { AlimentoId = idLeite,  Quantidade = 300, Unidade = "ml" },
                        new ItemRefeicao { AlimentoId = idOvo,    Quantidade = 3,   Unidade = "un" },
                        new ItemRefeicao { AlimentoId = idBanana, Quantidade = 1,   Unidade = "un" }
                    }
                },
                new Refeicao
                {
                    Tipo    = TipoRefeicao.Almoco,
                    Horario = new TimeOnly(12, 0),
                    Itens = new List<ItemRefeicao>
                    {
                        new ItemRefeicao { AlimentoId = idArroz,  Quantidade = 200, Unidade = "g" },
                        new ItemRefeicao { AlimentoId = idFeijao, Quantidade = 120, Unidade = "g" },
                        new ItemRefeicao { AlimentoId = idFrango, Quantidade = 200, Unidade = "g" },
                        new ItemRefeicao { AlimentoId = idBatata, Quantidade = 150, Unidade = "g" }
                    }
                },
                new Refeicao
                {
                    Tipo      = TipoRefeicao.LancheDaTarde,
                    Horario   = new TimeOnly(15, 30),
                    Observacoes = "Pos-treino: priorizar proteina e carboidrato",
                    Itens = new List<ItemRefeicao>
                    {
                        new ItemRefeicao { AlimentoId = idAveia, Quantidade = 60,  Unidade = "g"  },
                        new ItemRefeicao { AlimentoId = idLeite, Quantidade = 250, Unidade = "ml" },
                        new ItemRefeicao { AlimentoId = idOvo,   Quantidade = 3,   Unidade = "un" }
                    }
                },
                new Refeicao
                {
                    Tipo    = TipoRefeicao.Jantar,
                    Horario = new TimeOnly(19, 30),
                    Itens = new List<ItemRefeicao>
                    {
                        new ItemRefeicao { AlimentoId = idFrango, Quantidade = 180, Unidade = "g" },
                        new ItemRefeicao { AlimentoId = idArroz,  Quantidade = 150, Unidade = "g" }
                    }
                }
            }
        };

        _context.PlanosAlimentares.Add(planoCarlos);
        _context.PlanosAlimentares.Add(planoMariana);

        // ── Avaliacoes — Carlos (inicial + seguimento) ─────────────────────────
        var avaliacaoCarlosInicial = new AvaliacaoAntropometrica
        {
            Data               = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            ObservacoesClinicas = "Paciente sedentario, PA 130/85. Iniciando reeducacao alimentar.",
            Paciente           = carlos,
            Nutricionista      = nutricionista,
            MedidaCorporal = new MedidaCorporal
            {
                Peso   = 92m,
                Altura = 1.75m,
                CircunferenciasJson  = "{\"cintura\":98,\"quadril\":102,\"braco\":36,\"coxa\":58}",
                DobrasCutaneasJson   = "{\"triciptal\":18,\"subescapular\":22,\"biciptal\":12,\"suprailiaca\":25}"
            },
            ComposicaoCorporal = new ComposicaoCorporal
            {
                Imc              = 30.1m,
                PercentualGordura = 28m,
                MassaMagra       = 66.2m,
                Classificacao    = "Obesidade Grau I"
            }
        };

        var avaliacaoCarlosSeguimento = new AvaliacaoAntropometrica
        {
            Data               = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc),
            ObservacoesClinicas = "Boa adesao ao plano. PA 125/80. Reducao de 3kg em 10 dias.",
            Paciente           = carlos,
            Nutricionista      = nutricionista,
            MedidaCorporal = new MedidaCorporal
            {
                Peso   = 89m,
                Altura = 1.75m,
                CircunferenciasJson  = "{\"cintura\":95,\"quadril\":100,\"braco\":35,\"coxa\":56}",
                DobrasCutaneasJson   = "{\"triciptal\":16,\"subescapular\":19,\"biciptal\":11,\"suprailiaca\":22}"
            },
            ComposicaoCorporal = new ComposicaoCorporal
            {
                Imc              = 29.1m,
                PercentualGordura = 26m,
                MassaMagra       = 65.9m,
                Classificacao    = "Sobrepeso"
            }
        };

        // ── Avaliacao — Mariana (inicial) ──────────────────────────────────────
        var avaliacaoMarianaInicial = new AvaliacaoAntropometrica
        {
            Data               = new DateTime(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc),
            ObservacoesClinicas = "Paciente ativa, musculacao 3x/semana. Boa composicao corporal de base.",
            Paciente           = mariana,
            Nutricionista      = nutricionista,
            MedidaCorporal = new MedidaCorporal
            {
                Peso   = 58m,
                Altura = 1.63m,
                CircunferenciasJson  = "{\"cintura\":68,\"quadril\":92,\"braco\":29,\"coxa\":52}",
                DobrasCutaneasJson   = "{\"triciptal\":14,\"subescapular\":12,\"biciptal\":9,\"suprailiaca\":16}"
            },
            ComposicaoCorporal = new ComposicaoCorporal
            {
                Imc              = 21.8m,
                PercentualGordura = 22m,
                MassaMagra       = 45.2m,
                Classificacao    = "Eutrofico"
            }
        };

        _context.Avaliacoes.Add(avaliacaoCarlosInicial);
        _context.Avaliacoes.Add(avaliacaoCarlosSeguimento);
        _context.Avaliacoes.Add(avaliacaoMarianaInicial);

        // ── Metas — Carlos ─────────────────────────────────────────────────────
        var metaCarlosPeso = new Meta
        {
            Tipo      = TipoMeta.PerdaDePeso,
            ValorAlvo = 78m,
            Unidade   = "kg",
            Prazo     = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            Status    = StatusMeta.EmAndamento,
            Descricao = "Reduzir de 92kg para 78kg em 6 meses.",
            Paciente  = carlos,
            Progressos = new List<ProgressoMeta>
            {
                new ProgressoMeta { DataVerificacao = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),  ValorAtual = 92m, Observacao = "Peso inicial" },
                new ProgressoMeta { DataVerificacao = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc), ValorAtual = 89m, Observacao = "Boa evolucao em 10 dias" }
            }
        };

        var metaCarlosGordura = new Meta
        {
            Tipo      = TipoMeta.ReducaoGorduraCorporal,
            ValorAlvo = 18m,
            Unidade   = "%",
            Prazo     = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            Status    = StatusMeta.EmAndamento,
            Descricao = "Reduzir percentual de gordura de 28% para 18%.",
            Paciente  = carlos,
            Progressos = new List<ProgressoMeta>
            {
                new ProgressoMeta { DataVerificacao = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),  ValorAtual = 28m, Observacao = "Percentual inicial" },
                new ProgressoMeta { DataVerificacao = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc), ValorAtual = 26m, Observacao = "Reducao de 2% em 10 dias" }
            }
        };

        // ── Meta — Mariana ──────────────────────────────────────────────────────
        var metaMarianaMassa = new Meta
        {
            Tipo      = TipoMeta.GanhoDeMassa,
            ValorAlvo = 50m,
            Unidade   = "kg",
            Prazo     = new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc),
            Status    = StatusMeta.EmAndamento,
            Descricao = "Aumentar massa magra de 45,2kg para 50kg em 4 meses.",
            Paciente  = mariana,
            Progressos = new List<ProgressoMeta>
            {
                new ProgressoMeta { DataVerificacao = new DateTime(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc), ValorAtual = 45.2m, Observacao = "Massa magra inicial" }
            }
        };

        _context.Metas.Add(metaCarlosPeso);
        _context.Metas.Add(metaCarlosGordura);
        _context.Metas.Add(metaMarianaMassa);

        _logger.LogInformation(
            "Seed de demo concluido: 1 nutricionista, 2 pacientes, 2 planos alimentares, 3 avaliacoes, 3 metas.");
    }
}
