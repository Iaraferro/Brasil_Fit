namespace BrasilFit.Domain.Entities;

// Atende ao UC26 - rastreabilidade de operacoes sensiveis no sistema.
public class LogAuditoria
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
    public string Operacao { get; set; } = string.Empty;   // "CRIAR_PACIENTE", "EXCLUIR_PLANO" etc.
    public string Entidade { get; set; } = string.Empty;   // Nome do tipo afetado
    public string? EntidadeId { get; set; }                // Id do registro afetado
    public string? Detalhes { get; set; }                  // Payload em JSON (opcional)
    public string? EnderecoIp { get; set; }

    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
}
