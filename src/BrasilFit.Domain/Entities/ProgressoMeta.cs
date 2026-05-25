namespace BrasilFit.Domain.Entities;

public class ProgressoMeta
{
    public int Id { get; set; }
    public DateTime DataVerificacao { get; set; }
    public decimal ValorAtual { get; set; }
    public string? Observacao { get; set; }

    public int MetaId { get; set; }
    public Meta Meta { get; set; } = null!;
}
