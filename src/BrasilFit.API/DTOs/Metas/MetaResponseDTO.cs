using BrasilFit.Domain.Enums;

namespace BrasilFit.API.DTOs.Metas
{
    public class MetaResponseDTO
    {
        public int Id { get; set; }
        public TipoMeta Tipo { get; set; }
        public decimal ValorAlvo { get; set; }
        public string? Unidade { get; set; }
        public DateTime Prazo { get; set; }
        public StatusMeta Status { get; set; }
        public string? Descricao { get; set; }
        public DateTime CriadaEm { get; set; }
        public int PacienteId { get; set; }
        public string PacienteNome { get; set; } = string.Empty;
        public decimal? ProgressoAtual { get; set; }
        public decimal? PercentualConcluido { get; set; }

        public List<ProgressoMetaResponseDTO> Progressos { get; set; } = new();
    }

    public class ProgressoMetaResponseDTO
    {
        public int Id { get; set; }
        public DateTime DataVerificacao { get; set; }
        public decimal ValorAtual { get; set; }
        public string? Observacao { get; set; }
    }
}
