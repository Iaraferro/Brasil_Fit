using BrasilFit.Domain.Enums;

namespace BrasilFit.Domain.Entities;

public class Refeicao
{
    public int Id { get; set; }
    public TipoRefeicao Tipo { get; set; }
    public TimeOnly Horario { get; set; }
    public string? Observacoes { get; set; }

    public int PlanoAlimentarId { get; set; }
    public PlanoAlimentar PlanoAlimentar { get; set; } = null!;

    // N:N com Alimento atraves da entidade associativa ItemRefeicao,
    // necessaria por carregar atributos proprios (Quantidade, Unidade).
    public ICollection<ItemRefeicao> Itens { get; set; } = new List<ItemRefeicao>();
}
