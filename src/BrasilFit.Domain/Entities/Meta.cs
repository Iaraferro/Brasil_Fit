using BrasilFit.Domain.Enums;

namespace BrasilFit.Domain.Entities;

public class Meta
{
    public int Id { get; set; }
    public TipoMeta Tipo { get; set; }
    public decimal ValorAlvo { get; set; }
    public string? Unidade { get; set; }  // kg, %, cm
    public DateTime Prazo { get; set; }
    public StatusMeta Status { get; set; } = StatusMeta.EmAndamento;
    public string? Descricao { get; set; }
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; } = null!;

    public ICollection<ProgressoMeta> Progressos { get; set; } = new List<ProgressoMeta>();
}
