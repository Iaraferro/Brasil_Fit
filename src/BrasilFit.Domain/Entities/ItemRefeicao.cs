namespace BrasilFit.Domain.Entities;

// Entidade associativa entre Refeicao e Alimento.
// Modela o relacionamento N:N com atributos proprios (Quantidade, Unidade).
public class ItemRefeicao
{
    public int Id { get; set; }
    public decimal Quantidade { get; set; }
    public string Unidade { get; set; } = "g"; // g, ml, un, colher

    public int RefeicaoId { get; set; }
    public Refeicao Refeicao { get; set; } = null!;

    public int AlimentoId { get; set; }
    public Alimento Alimento { get; set; } = null!;
}
