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

        await SeedDadosExtendidosAsync(ct);
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

    private async Task SeedDadosExtendidosAsync(CancellationToken ct)
    {
        // So executa se ainda nao temos os nutricionistas adicionais.
        if (await _context.Nutricionistas.CountAsync(ct) >= 3)
        {
            _logger.LogInformation("Dados extendidos ja existem. Pulando.");
            return;
        }

        _logger.LogInformation("Inserindo dados extendidos (mais nutricionistas e pacientes)...");

        var alimentoIds = await _context.Alimentos.Select(a => a.Id).ToListAsync(ct);
        if (alimentoIds.Count < 10) return;

        int idArroz = alimentoIds[0], idFeijao = alimentoIds[1], idFrango = alimentoIds[2];
        int idOvo   = alimentoIds[3], idBanana = alimentoIds[4];
        int idPao   = alimentoIds[6], idLeite  = alimentoIds[7], idBatata = alimentoIds[8];
        int idAveia = alimentoIds[9];

        static DateTime Utc(int y, int m, int d) => new(y, m, d, 0, 0, 0, DateTimeKind.Utc);

        // ── Nutricionistas adicionais ────────────────────────────────────────────
        var ricardo = new Nutricionista
        {
            Nome          = "Dr. Ricardo Santos",
            Email         = "ricardo.santos@brasilfit.local",
            SenhaHash     = _hashSenha("Nutri@123"),
            Crn           = "23456/SP",
            Especialidade = "Nutricao Clinica",
            Ativo         = true,
            CriadoEm     = Utc(2025, 11, 10)
        };

        var beatriz = new Nutricionista
        {
            Nome          = "Dra. Beatriz Oliveira",
            Email         = "beatriz.oliveira@brasilfit.local",
            SenhaHash     = _hashSenha("Nutri@123"),
            Crn           = "34567/RJ",
            Especialidade = "Nutricao Funcional",
            Ativo         = true,
            CriadoEm     = Utc(2025, 11, 20)
        };

        _context.Nutricionistas.Add(ricardo);
        _context.Nutricionistas.Add(beatriz);

        // ── Pacientes de Ricardo Santos ──────────────────────────────────────────

        // Fernanda Costa — diabetica, precisa emagrecer urgente (paciente antiga)
        var fernanda = new Paciente
        {
            Nome             = "Fernanda Costa",
            Email            = "fernanda.costa@brasilfit.local",
            SenhaHash        = _hashSenha("Paciente@123"),
            DataNascimento   = Utc(1982, 4, 8),
            Sexo             = Sexo.Feminino,
            Telefone         = "(11) 97654-3210",
            HistoricoClinico = "Diabetes mellitus tipo 2 diagnosticado em 2020. Hipertensao controlada. Sedentaria.",
            Nutricionista    = ricardo,
            Ativo            = true,
            CriadoEm        = Utc(2025, 12, 3),
            Endereco = new Endereco { Cep="01023-010", Logradouro="Rua Direita", Numero="200", Bairro="Centro", Cidade="Sao Paulo", Uf="SP" }
        };

        // Joao Pedro Almeida — musculacao, quer hipertrofia
        var joaoPedro = new Paciente
        {
            Nome             = "Joao Pedro Almeida",
            Email            = "joao.almeida@brasilfit.local",
            SenhaHash        = _hashSenha("Paciente@123"),
            DataNascimento   = Utc(1998, 9, 15),
            Sexo             = Sexo.Masculino,
            Telefone         = "(21) 98888-1234",
            HistoricoClinico = "Saudavel. Pratica musculacao 5x/semana ha 2 anos. Busca ganho de massa.",
            Nutricionista    = ricardo,
            Ativo            = true,
            CriadoEm        = Utc(2026, 1, 7),
            Endereco = new Endereco { Cep="20040-020", Logradouro="Av. Rio Branco", Numero="50", Bairro="Centro", Cidade="Rio de Janeiro", Uf="RJ" }
        };

        // Lucia Mendes — colesterol alto, menopausa
        var lucia = new Paciente
        {
            Nome             = "Lucia Mendes",
            Email            = "lucia.mendes@brasilfit.local",
            SenhaHash        = _hashSenha("Paciente@123"),
            DataNascimento   = Utc(1969, 6, 22),
            Sexo             = Sexo.Feminino,
            Telefone         = "(11) 94321-8765",
            HistoricoClinico = "Colesterol LDL elevado (180 mg/dL). Pos-menopausa. Historico familiar de cardiopatia.",
            Nutricionista    = ricardo,
            Ativo            = true,
            CriadoEm        = Utc(2026, 2, 14),
            Endereco = new Endereco { Cep="04002-000", Logradouro="Rua Vergueiro", Numero="800", Bairro="Vila Mariana", Cidade="Sao Paulo", Uf="SP" }
        };

        // Rafael Nogueira — obesidade, iniciando tratamento
        var rafael = new Paciente
        {
            Nome             = "Rafael Nogueira",
            Email            = "rafael.nogueira@brasilfit.local",
            SenhaHash        = _hashSenha("Paciente@123"),
            DataNascimento   = Utc(1993, 11, 30),
            Sexo             = Sexo.Masculino,
            Telefone         = "(11) 95555-7890",
            HistoricoClinico = "Obesidade grau I. Trabalha em home office. Alimentacao majoritariamente ultraprocessada.",
            Nutricionista    = ricardo,
            Ativo            = true,
            CriadoEm        = Utc(2026, 3, 5),
            Endereco = new Endereco { Cep="13010-110", Logradouro="Rua Barao de Jaguara", Numero="960", Bairro="Centro", Cidade="Campinas", Uf="SP" }
        };

        _context.Pacientes.Add(fernanda);
        _context.Pacientes.Add(joaoPedro);
        _context.Pacientes.Add(lucia);
        _context.Pacientes.Add(rafael);

        // ── Pacientes de Beatriz Oliveira ────────────────────────────────────────

        var sofia = new Paciente
        {
            Nome             = "Sofia Torres",
            Email            = "sofia.torres@brasilfit.local",
            SenhaHash        = _hashSenha("Paciente@123"),
            DataNascimento   = Utc(2002, 3, 12),
            Sexo             = Sexo.Feminino,
            Telefone         = "(21) 93333-4567",
            HistoricoClinico = "Vegetariana ha 3 anos. Saudavel. Quer manter peso e melhorar performance no crossfit.",
            Nutricionista    = beatriz,
            Ativo            = true,
            CriadoEm        = Utc(2026, 4, 2),
            Endereco = new Endereco { Cep="22250-040", Logradouro="Rua Voluntarios da Patria", Numero="190", Bairro="Botafogo", Cidade="Rio de Janeiro", Uf="RJ" }
        };

        var pedro = new Paciente
        {
            Nome             = "Pedro Henrique Lima",
            Email            = "pedro.lima@brasilfit.local",
            SenhaHash        = _hashSenha("Paciente@123"),
            DataNascimento   = Utc(1988, 7, 18),
            Sexo             = Sexo.Masculino,
            Telefone         = "(31) 92222-6543",
            HistoricoClinico = "Pre-diabetes (glicemia em jejum 105 mg/dL). Sobrepeso. Estresse cronico no trabalho.",
            Nutricionista    = beatriz,
            Ativo            = true,
            CriadoEm        = Utc(2026, 5, 10),
            Endereco = new Endereco { Cep="30112-000", Logradouro="Av. Afonso Pena", Numero="1500", Bairro="Centro", Cidade="Belo Horizonte", Uf="MG" }
        };

        _context.Pacientes.Add(sofia);
        _context.Pacientes.Add(pedro);

        // ── Planos alimentares ───────────────────────────────────────────────────
        _context.PlanosAlimentares.Add(new PlanoAlimentar
        {
            Nome          = "Controle Glicemico - Fernanda",
            Objetivo      = "Reducao da glicemia e perda de peso com baixo indice glicemico",
            DataInicio    = Utc(2025, 12, 10),
            DuracaoDias   = 180,
            Observacoes   = "Fracionar em 6 refeicoes. Evitar acucar e farinhas refinadas. Priorizar fibras.",
            Ativo         = true,
            Paciente      = fernanda,
            Nutricionista = ricardo,
            Refeicoes = new List<Refeicao>
            {
                new() { Tipo = TipoRefeicao.CafeDaManha, Horario = new TimeOnly(7,0), Observacoes = "Sem acucar", Itens = new List<ItemRefeicao>
                    { new() { AlimentoId = idAveia, Quantidade = 40, Unidade = "g" }, new() { AlimentoId = idLeite, Quantidade = 200, Unidade = "ml" }, new() { AlimentoId = idBanana, Quantidade = 1, Unidade = "un" } }},
                new() { Tipo = TipoRefeicao.Almoco, Horario = new TimeOnly(12,0), Itens = new List<ItemRefeicao>
                    { new() { AlimentoId = idArroz, Quantidade = 120, Unidade = "g" }, new() { AlimentoId = idFeijao, Quantidade = 80, Unidade = "g" }, new() { AlimentoId = idFrango, Quantidade = 140, Unidade = "g" } }},
                new() { Tipo = TipoRefeicao.Jantar, Horario = new TimeOnly(19,0), Observacoes = "Refeicao leve", Itens = new List<ItemRefeicao>
                    { new() { AlimentoId = idFrango, Quantidade = 120, Unidade = "g" }, new() { AlimentoId = idBatata, Quantidade = 80, Unidade = "g" } }}
            }
        });

        _context.PlanosAlimentares.Add(new PlanoAlimentar
        {
            Nome          = "Hipertrofia Intermediaria - Joao Pedro",
            Objetivo      = "Ganho de massa muscular com deficit minimo de gordura",
            DataInicio    = Utc(2026, 1, 10),
            DuracaoDias   = 90,
            Observacoes   = "Ingestao proteica de 2g/kg. Pre e pos-treino fundamentais.",
            Ativo         = true,
            Paciente      = joaoPedro,
            Nutricionista = ricardo,
            Refeicoes = new List<Refeicao>
            {
                new() { Tipo = TipoRefeicao.CafeDaManha, Horario = new TimeOnly(7,30), Itens = new List<ItemRefeicao>
                    { new() { AlimentoId = idAveia, Quantidade = 100, Unidade = "g" }, new() { AlimentoId = idLeite, Quantidade = 300, Unidade = "ml" }, new() { AlimentoId = idOvo, Quantidade = 4, Unidade = "un" } }},
                new() { Tipo = TipoRefeicao.Almoco, Horario = new TimeOnly(12,30), Itens = new List<ItemRefeicao>
                    { new() { AlimentoId = idArroz, Quantidade = 250, Unidade = "g" }, new() { AlimentoId = idFeijao, Quantidade = 150, Unidade = "g" }, new() { AlimentoId = idFrango, Quantidade = 250, Unidade = "g" } }},
                new() { Tipo = TipoRefeicao.LancheDaTarde, Horario = new TimeOnly(16,0), Observacoes = "Pre-treino", Itens = new List<ItemRefeicao>
                    { new() { AlimentoId = idBanana, Quantidade = 2, Unidade = "un" }, new() { AlimentoId = idOvo, Quantidade = 3, Unidade = "un" } }},
                new() { Tipo = TipoRefeicao.Jantar, Horario = new TimeOnly(20,0), Observacoes = "Pos-treino", Itens = new List<ItemRefeicao>
                    { new() { AlimentoId = idFrango, Quantidade = 200, Unidade = "g" }, new() { AlimentoId = idArroz, Quantidade = 200, Unidade = "g" }, new() { AlimentoId = idBatata, Quantidade = 100, Unidade = "g" } }}
            }
        });

        _context.PlanosAlimentares.Add(new PlanoAlimentar
        {
            Nome          = "Cardioprotecao - Lucia",
            Objetivo      = "Reducao do colesterol LDL e triglicerides por meio da dieta",
            DataInicio    = Utc(2026, 2, 20),
            DuracaoDias   = 120,
            Observacoes   = "Priorizar gorduras insaturadas. Aumentar consumo de fibras soluveis. Evitar frituras.",
            Ativo         = true,
            Paciente      = lucia,
            Nutricionista = ricardo,
            Refeicoes = new List<Refeicao>
            {
                new() { Tipo = TipoRefeicao.CafeDaManha, Horario = new TimeOnly(7,0), Itens = new List<ItemRefeicao>
                    { new() { AlimentoId = idAveia, Quantidade = 50, Unidade = "g" }, new() { AlimentoId = idLeite, Quantidade = 200, Unidade = "ml" }, new() { AlimentoId = idPao, Quantidade = 1, Unidade = "un" } }},
                new() { Tipo = TipoRefeicao.Almoco, Horario = new TimeOnly(12,0), Itens = new List<ItemRefeicao>
                    { new() { AlimentoId = idArroz, Quantidade = 130, Unidade = "g" }, new() { AlimentoId = idFeijao, Quantidade = 100, Unidade = "g" }, new() { AlimentoId = idFrango, Quantidade = 150, Unidade = "g" } }},
                new() { Tipo = TipoRefeicao.Jantar, Horario = new TimeOnly(19,30), Itens = new List<ItemRefeicao>
                    { new() { AlimentoId = idFrango, Quantidade = 120, Unidade = "g" }, new() { AlimentoId = idOvo, Quantidade = 2, Unidade = "un" } }}
            }
        });

        // ── Avaliacoes com historico ─────────────────────────────────────────────

        // Fernanda: 3 avaliacoes mostrando evolucao positiva ao longo de 6 meses
        _context.Avaliacoes.Add(new AvaliacaoAntropometrica { Data = Utc(2025,12,10), ObservacoesClinicas = "Inicio do acompanhamento. Glicemia em jejum 148 mg/dL. Pressao 135/90.", Paciente = fernanda, Nutricionista = ricardo,
            MedidaCorporal = new MedidaCorporal { Peso=88m, Altura=1.62m, CircunferenciasJson="{\"cintura\":102,\"quadril\":108,\"braco\":34}", DobrasCutaneasJson="{\"triciptal\":28,\"subescapular\":30}" },
            ComposicaoCorporal = new ComposicaoCorporal { Imc=33.5m, PercentualGordura=38m, MassaMagra=54.6m, Classificacao="Obesidade Grau I" }});

        _context.Avaliacoes.Add(new AvaliacaoAntropometrica { Data = Utc(2026,3,5), ObservacoesClinicas = "Boa adesao. Glicemia 128 mg/dL. Pressao 128/85. Reducao de 6kg.", Paciente = fernanda, Nutricionista = ricardo,
            MedidaCorporal = new MedidaCorporal { Peso=82m, Altura=1.62m, CircunferenciasJson="{\"cintura\":96,\"quadril\":104,\"braco\":32}", DobrasCutaneasJson="{\"triciptal\":24,\"subescapular\":26}" },
            ComposicaoCorporal = new ComposicaoCorporal { Imc=31.2m, PercentualGordura=34m, MassaMagra=54.1m, Classificacao="Obesidade Grau I" }});

        _context.Avaliacoes.Add(new AvaliacaoAntropometrica { Data = Utc(2026,6,5), ObservacoesClinicas = "Excelente evolucao. Glicemia 108 mg/dL. Medico reduziu dose de medicamento.", Paciente = fernanda, Nutricionista = ricardo,
            MedidaCorporal = new MedidaCorporal { Peso=75m, Altura=1.62m, CircunferenciasJson="{\"cintura\":90,\"quadril\":99,\"braco\":30}", DobrasCutaneasJson="{\"triciptal\":20,\"subescapular\":22}" },
            ComposicaoCorporal = new ComposicaoCorporal { Imc=28.6m, PercentualGordura=29m, MassaMagra=53.2m, Classificacao="Sobrepeso" }});

        // Joao Pedro: 2 avaliacoes mostrando ganho de massa
        _context.Avaliacoes.Add(new AvaliacaoAntropometrica { Data = Utc(2026,1,10), ObservacoesClinicas = "Bom nivel de condicionamento fisico. Objetivo claro de hipertrofia.", Paciente = joaoPedro, Nutricionista = ricardo,
            MedidaCorporal = new MedidaCorporal { Peso=72m, Altura=1.80m, CircunferenciasJson="{\"cintura\":80,\"quadril\":96,\"braco\":36,\"coxa\":55}" },
            ComposicaoCorporal = new ComposicaoCorporal { Imc=22.2m, PercentualGordura=12m, MassaMagra=63.4m, Classificacao="Eutrofico" }});

        _context.Avaliacoes.Add(new AvaliacaoAntropometrica { Data = Utc(2026,4,15), ObservacoesClinicas = "Ganho de 6kg em 3 meses. Percentual de gordura mantido. Excelente resultado.", Paciente = joaoPedro, Nutricionista = ricardo,
            MedidaCorporal = new MedidaCorporal { Peso=78m, Altura=1.80m, CircunferenciasJson="{\"cintura\":82,\"quadril\":98,\"braco\":39,\"coxa\":57}" },
            ComposicaoCorporal = new ComposicaoCorporal { Imc=24.1m, PercentualGordura=11m, MassaMagra=69.4m, Classificacao="Eutrofico" }});

        // Lucia: 2 avaliacoes mostrando evolucao do colesterol
        _context.Avaliacoes.Add(new AvaliacaoAntropometrica { Data = Utc(2026,2,20), ObservacoesClinicas = "Colesterol LDL 180 mg/dL, HDL 42. Sedentaria. Dieta rica em gorduras saturadas.", Paciente = lucia, Nutricionista = ricardo,
            MedidaCorporal = new MedidaCorporal { Peso=74m, Altura=1.58m, CircunferenciasJson="{\"cintura\":92,\"quadril\":100,\"braco\":31}" },
            ComposicaoCorporal = new ComposicaoCorporal { Imc=29.7m, PercentualGordura=42m, MassaMagra=42.9m, Classificacao="Sobrepeso" }});

        _context.Avaliacoes.Add(new AvaliacaoAntropometrica { Data = Utc(2026,5,28), ObservacoesClinicas = "Colesterol LDL 142 mg/dL (reducao de 38 pontos!). HDL 48. Paciente muito motivada.", Paciente = lucia, Nutricionista = ricardo,
            MedidaCorporal = new MedidaCorporal { Peso=70m, Altura=1.58m, CircunferenciasJson="{\"cintura\":87,\"quadril\":97,\"braco\":29}" },
            ComposicaoCorporal = new ComposicaoCorporal { Imc=28.1m, PercentualGordura=38m, MassaMagra=43.4m, Classificacao="Sobrepeso" }});

        // Rafael: avaliacao inicial
        _context.Avaliacoes.Add(new AvaliacaoAntropometrica { Data = Utc(2026,3,10), ObservacoesClinicas = "Primeira consulta. Muito motivado apos susto com pre-diabetes do colega de trabalho.", Paciente = rafael, Nutricionista = ricardo,
            MedidaCorporal = new MedidaCorporal { Peso=96m, Altura=1.75m, CircunferenciasJson="{\"cintura\":104,\"quadril\":106,\"braco\":36}" },
            ComposicaoCorporal = new ComposicaoCorporal { Imc=31.3m, PercentualGordura=30m, MassaMagra=67.2m, Classificacao="Obesidade Grau I" }});

        // Sofia e Pedro: avaliacao inicial
        _context.Avaliacoes.Add(new AvaliacaoAntropometrica { Data = Utc(2026,4,8), ObservacoesClinicas = "Exame de sangue sem alteracoes. Boa ingestao de proteinas via leguminosas. Crossfit 4x/semana.", Paciente = sofia, Nutricionista = beatriz,
            MedidaCorporal = new MedidaCorporal { Peso=54m, Altura=1.67m, CircunferenciasJson="{\"cintura\":65,\"quadril\":90,\"braco\":27}" },
            ComposicaoCorporal = new ComposicaoCorporal { Imc=19.4m, PercentualGordura=18m, MassaMagra=44.3m, Classificacao="Eutrofico" }});

        _context.Avaliacoes.Add(new AvaliacaoAntropometrica { Data = Utc(2026,5,15), ObservacoesClinicas = "Glicemia em jejum 105 mg/dL. Circunferencia abdominal elevada. Estresse como fator agravante.", Paciente = pedro, Nutricionista = beatriz,
            MedidaCorporal = new MedidaCorporal { Peso=85m, Altura=1.72m, CircunferenciasJson="{\"cintura\":98,\"quadril\":102,\"braco\":34}" },
            ComposicaoCorporal = new ComposicaoCorporal { Imc=28.7m, PercentualGordura=27m, MassaMagra=62.1m, Classificacao="Sobrepeso" }});

        // ── Metas ────────────────────────────────────────────────────────────────
        _context.Metas.Add(new Meta { Tipo=TipoMeta.PerdaDePeso, ValorAlvo=68m, Unidade="kg", Prazo=Utc(2026,9,1), Status=StatusMeta.EmAndamento,
            Descricao="Reducao de 88kg para 68kg com controle glicemico.", Paciente=fernanda,
            Progressos = new List<ProgressoMeta>
            {
                new() { DataVerificacao=Utc(2025,12,10), ValorAtual=88m, Observacao="Peso inicial" },
                new() { DataVerificacao=Utc(2026,3,5),   ValorAtual=82m, Observacao="Reducao consistente" },
                new() { DataVerificacao=Utc(2026,6,5),   ValorAtual=75m, Observacao="Meta parcial atingida (fase 1)" }
            }});

        _context.Metas.Add(new Meta { Tipo=TipoMeta.GanhoDeMassa, ValorAlvo=85m, Unidade="kg", Prazo=Utc(2026,8,1), Status=StatusMeta.EmAndamento,
            Descricao="Ganhar 13kg de peso corporal com maxima preservacao muscular.", Paciente=joaoPedro,
            Progressos = new List<ProgressoMeta>
            {
                new() { DataVerificacao=Utc(2026,1,10), ValorAtual=72m, Observacao="Peso inicial" },
                new() { DataVerificacao=Utc(2026,4,15), ValorAtual=78m, Observacao="+6kg em 3 meses" }
            }});

        _context.Metas.Add(new Meta { Tipo=TipoMeta.PerdaDePeso, ValorAlvo=62m, Unidade="kg", Prazo=Utc(2026,8,1), Status=StatusMeta.EmAndamento,
            Descricao="Reducao de 74kg para 62kg com foco em saude cardiovascular.", Paciente=lucia,
            Progressos = new List<ProgressoMeta>
            {
                new() { DataVerificacao=Utc(2026,2,20), ValorAtual=74m, Observacao="Peso inicial" },
                new() { DataVerificacao=Utc(2026,5,28), ValorAtual=70m, Observacao="-4kg com reducao do colesterol" }
            }});

        _context.Metas.Add(new Meta { Tipo=TipoMeta.PerdaDePeso, ValorAlvo=80m, Unidade="kg", Prazo=Utc(2026,12,1), Status=StatusMeta.EmAndamento,
            Descricao="Reducao de 96kg para 80kg com habitos sustentaveis.", Paciente=rafael,
            Progressos = new List<ProgressoMeta>
            { new() { DataVerificacao=Utc(2026,3,10), ValorAtual=96m, Observacao="Peso inicial" } }});

        _context.Metas.Add(new Meta { Tipo=TipoMeta.Manutencao, ValorAlvo=54m, Unidade="kg", Prazo=Utc(2026,12,1), Status=StatusMeta.EmAndamento,
            Descricao="Manter peso atual e melhorar composicao corporal.", Paciente=sofia,
            Progressos = new List<ProgressoMeta>
            { new() { DataVerificacao=Utc(2026,4,8), ValorAtual=54m, Observacao="Peso inicial" } }});

        _context.Metas.Add(new Meta { Tipo=TipoMeta.ReducaoGorduraCorporal, ValorAlvo=20m, Unidade="%", Prazo=Utc(2026,11,1), Status=StatusMeta.EmAndamento,
            Descricao="Reduzir gordura visceral para normalizar glicemia.", Paciente=pedro,
            Progressos = new List<ProgressoMeta>
            { new() { DataVerificacao=Utc(2026,5,15), ValorAtual=27m, Observacao="Percentual inicial" } }});

        _logger.LogInformation(
            "Seed extendido concluido: +2 nutricionistas, +6 pacientes, +3 planos, +10 avaliacoes, +6 metas.");
    }
}
