using BrasilFit.Domain.Enums;

namespace BrasilFit.API.DTOs.Metas
{
    public class MetaRequestDTO
    {
        public TipoMeta Tipo { get; set; }
        public decimal ValorAlvo { get; set; }
        public string? Unidade { get; set; }
        public DateTime Prazo { get; set; }
        public string? Descricao { get; set; }
        public int PacienteId { get; set; }
    }

    public class ProgressoMetaRequestDTO
    {
        public decimal ValorAtual { get; set; }
        public string? Observacao { get; set; }
    }
}
