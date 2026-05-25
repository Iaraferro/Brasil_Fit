namespace BrasilFit.API.DTOs.Common;

// Envelope padrao para qualquer endpoint que retorna lista paginada.
// O front recebe os itens da pagina + metadados para montar a paginacao.
public class PaginacaoResultadoDto<T>
{
    public IReadOnlyList<T> Itens { get; init; } = Array.Empty<T>();
    public int Pagina { get; init; }
    public int TamanhoPagina { get; init; }
    public int TotalItens { get; init; }

    public int TotalPaginas => TamanhoPagina == 0
        ? 0
        : (int)Math.Ceiling((double)TotalItens / TamanhoPagina);

    public bool TemAnterior => Pagina > 1;
    public bool TemProxima => Pagina < TotalPaginas;

    public static PaginacaoResultadoDto<T> Criar(
        IReadOnlyList<T> itens, int pagina, int tamanhoPagina, int totalItens) => new()
    {
        Itens = itens,
        Pagina = pagina,
        TamanhoPagina = tamanhoPagina,
        TotalItens = totalItens
    };
}
