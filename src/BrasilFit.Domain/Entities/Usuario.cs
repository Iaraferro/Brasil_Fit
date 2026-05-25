using BrasilFit.Domain.Enums;

namespace BrasilFit.Domain.Entities;

// Classe base da hierarquia de usuarios.
// Mapeada via TPH (Table-per-Hierarchy): todos os tipos vivem na tabela "Usuarios"
// com a coluna discriminadora "Papel".
public abstract class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public PapelUsuario Papel { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
    public ICollection<LogAuditoria> LogsAuditoria { get; set; } = new List<LogAuditoria>();
}
