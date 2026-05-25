using BrasilFit.Domain.Entities;
using BrasilFit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BrasilFit.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Hierarquia de Usuario (TPH).
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Administrador> Administradores => Set<Administrador>();
    public DbSet<Nutricionista> Nutricionistas => Set<Nutricionista>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();

    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<PlanoAlimentar> PlanosAlimentares => Set<PlanoAlimentar>();
    public DbSet<Refeicao> Refeicoes => Set<Refeicao>();
    public DbSet<ItemRefeicao> ItensRefeicao => Set<ItemRefeicao>();
    public DbSet<Alimento> Alimentos => Set<Alimento>();
    public DbSet<AvaliacaoAntropometrica> Avaliacoes => Set<AvaliacaoAntropometrica>();
    public DbSet<MedidaCorporal> MedidasCorporais => Set<MedidaCorporal>();
    public DbSet<ComposicaoCorporal> ComposicoesCorporais => Set<ComposicaoCorporal>();
    public DbSet<Meta> Metas => Set<Meta>();
    public DbSet<ProgressoMeta> ProgressosMeta => Set<ProgressoMeta>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =====================================================================
        // Hierarquia Usuario - TPH (Table-per-Hierarchy)
        // =====================================================================
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(u => u.Id);

            e.Property(u => u.Nome).IsRequired().HasMaxLength(150);
            e.Property(u => u.Email).IsRequired().HasMaxLength(180);
            e.Property(u => u.SenhaHash).IsRequired().HasMaxLength(300);
            e.Property(u => u.Papel).HasConversion<int>();

            e.HasIndex(u => u.Email).IsUnique();

            // Discriminador da TPH.
            e.HasDiscriminator<PapelUsuario>("Papel")
                .HasValue<Administrador>(PapelUsuario.Administrador)
                .HasValue<Nutricionista>(PapelUsuario.Nutricionista)
                .HasValue<Paciente>(PapelUsuario.Paciente);
        });

        modelBuilder.Entity<Nutricionista>(e =>
        {
            e.Property(n => n.Crn).HasMaxLength(20);
            e.HasIndex(n => n.Crn).IsUnique().HasFilter("[Crn] IS NOT NULL");
        });

        modelBuilder.Entity<Paciente>(e =>
        {
            e.Property(p => p.Telefone).HasMaxLength(20);
            e.Property(p => p.HistoricoClinico).HasMaxLength(2000);

            // Paciente -> Nutricionista (N:1) opcional.
            e.HasOne(p => p.Nutricionista)
                .WithMany(n => n.Pacientes)
                .HasForeignKey(p => p.NutricionistaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =====================================================================
        // Endereco - 1:1 com Paciente
        // =====================================================================
        modelBuilder.Entity<Endereco>(e =>
        {
            e.HasKey(en => en.Id);
            e.Property(en => en.Cep).IsRequired().HasMaxLength(9);
            e.Property(en => en.Logradouro).IsRequired().HasMaxLength(200);
            e.Property(en => en.Numero).IsRequired().HasMaxLength(20);
            e.Property(en => en.Complemento).HasMaxLength(100);
            e.Property(en => en.Bairro).IsRequired().HasMaxLength(100);
            e.Property(en => en.Cidade).IsRequired().HasMaxLength(100);
            e.Property(en => en.Uf).IsRequired().HasMaxLength(2).IsFixedLength();

            e.HasOne(en => en.Paciente)
                .WithOne(p => p.Endereco)
                .HasForeignKey<Endereco>(en => en.PacienteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // Alimento
        // =====================================================================
        modelBuilder.Entity<Alimento>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Nome).IsRequired().HasMaxLength(150);
            e.Property(a => a.Marca).HasMaxLength(100);
            e.Property(a => a.CodigoBarrasExterno).HasMaxLength(50);

            e.Property(a => a.Kcal).HasPrecision(8, 2);
            e.Property(a => a.Carboidratos).HasPrecision(8, 2);
            e.Property(a => a.Proteinas).HasPrecision(8, 2);
            e.Property(a => a.Lipidios).HasPrecision(8, 2);

            e.HasIndex(a => a.CodigoBarrasExterno).IsUnique().HasFilter("[CodigoBarrasExterno] IS NOT NULL");
            e.HasIndex(a => a.Nome);
        });

        // =====================================================================
        // PlanoAlimentar
        // =====================================================================
        modelBuilder.Entity<PlanoAlimentar>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Nome).IsRequired().HasMaxLength(150);
            e.Property(p => p.Objetivo).HasMaxLength(300);
            e.Property(p => p.Observacoes).HasMaxLength(2000);

            e.HasOne(p => p.Paciente)
                .WithMany(pa => pa.PlanosAlimentares)
                .HasForeignKey(p => p.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Nutricionista)
                .WithMany(n => n.PlanosAlimentares)
                .HasForeignKey(p => p.NutricionistaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =====================================================================
        // Refeicao -> ItemRefeicao -> Alimento (N:N com payload)
        // =====================================================================
        modelBuilder.Entity<Refeicao>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Tipo).HasConversion<int>();
            e.Property(r => r.Observacoes).HasMaxLength(500);

            e.HasOne(r => r.PlanoAlimentar)
                .WithMany(p => p.Refeicoes)
                .HasForeignKey(r => r.PlanoAlimentarId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemRefeicao>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Quantidade).HasPrecision(8, 2);
            e.Property(i => i.Unidade).IsRequired().HasMaxLength(20);

            e.HasOne(i => i.Refeicao)
                .WithMany(r => r.Itens)
                .HasForeignKey(i => i.RefeicaoId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(i => i.Alimento)
                .WithMany(a => a.ItensRefeicao)
                .HasForeignKey(i => i.AlimentoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =====================================================================
        // Avaliacao + MedidaCorporal + ComposicaoCorporal
        // =====================================================================
        modelBuilder.Entity<AvaliacaoAntropometrica>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.ObservacoesClinicas).HasMaxLength(2000);

            e.HasOne(a => a.Paciente)
                .WithMany(p => p.Avaliacoes)
                .HasForeignKey(a => a.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Nutricionista)
                .WithMany(n => n.Avaliacoes)
                .HasForeignKey(a => a.NutricionistaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MedidaCorporal>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Peso).HasPrecision(6, 2);
            e.Property(m => m.Altura).HasPrecision(4, 2);

            e.HasOne(m => m.AvaliacaoAntropometrica)
                .WithOne(a => a.MedidaCorporal)
                .HasForeignKey<MedidaCorporal>(m => m.AvaliacaoAntropometricaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ComposicaoCorporal>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Imc).HasPrecision(5, 2);
            e.Property(c => c.PercentualGordura).HasPrecision(5, 2);
            e.Property(c => c.MassaMagra).HasPrecision(6, 2);
            e.Property(c => c.Classificacao).HasMaxLength(50);

            e.HasOne(c => c.AvaliacaoAntropometrica)
                .WithOne(a => a.ComposicaoCorporal)
                .HasForeignKey<ComposicaoCorporal>(c => c.AvaliacaoAntropometricaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // Meta + ProgressoMeta
        // =====================================================================
        modelBuilder.Entity<Meta>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Tipo).HasConversion<int>();
            e.Property(m => m.Status).HasConversion<int>();
            e.Property(m => m.ValorAlvo).HasPrecision(10, 2);
            e.Property(m => m.Unidade).HasMaxLength(20);
            e.Property(m => m.Descricao).HasMaxLength(500);

            e.HasOne(m => m.Paciente)
                .WithMany(p => p.Metas)
                .HasForeignKey(m => m.PacienteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProgressoMeta>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.ValorAtual).HasPrecision(10, 2);
            e.Property(p => p.Observacao).HasMaxLength(500);

            e.HasOne(p => p.Meta)
                .WithMany(m => m.Progressos)
                .HasForeignKey(p => p.MetaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // Notificacao
        // =====================================================================
        modelBuilder.Entity<Notificacao>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Titulo).IsRequired().HasMaxLength(150);
            e.Property(n => n.Mensagem).IsRequired().HasMaxLength(1000);

            e.HasOne(n => n.Usuario)
                .WithMany(u => u.Notificacoes)
                .HasForeignKey(n => n.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // LogAuditoria (UC26)
        // =====================================================================
        modelBuilder.Entity<LogAuditoria>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Operacao).IsRequired().HasMaxLength(100);
            e.Property(l => l.Entidade).IsRequired().HasMaxLength(100);
            e.Property(l => l.EntidadeId).HasMaxLength(50);
            e.Property(l => l.Detalhes).HasMaxLength(4000);
            e.Property(l => l.EnderecoIp).HasMaxLength(45);

            e.HasOne(l => l.Usuario)
                .WithMany(u => u.LogsAuditoria)
                .HasForeignKey(l => l.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(l => l.DataHora);
            e.HasIndex(l => l.Operacao);
        });
    }
}
