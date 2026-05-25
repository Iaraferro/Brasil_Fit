namespace BrasilFit.API.DTOs.Common;

// Recebida via [FromQuery]. Os setters fazem clamp para evitar valores absurdos
// vindos do cliente (ex.: TamanhoPagina = 999999 derrubaria o servidor).
public class PaginacaoQuery
{
    public const int TamanhoPaginaMaximo = 100;

    private int _pagina = 1;
    private int _tamanhoPagina = 20;

    public int Pagina
    {
        get => _pagina;
        set => _pagina = value < 1 ? 1 : value;
    }

    public int TamanhoPagina
    {
        get => _tamanhoPagina;
        set => _tamanhoPagina = value switch
        {
            < 1                     => 20,
            > TamanhoPaginaMaximo   => TamanhoPaginaMaximo,
            _                       => value
        };
    }

    // Termo livre de busca - cada controller decide em quais campos aplicar.
    public string? Busca { get; set; }

    // Ordenacao opcional: nome do campo + direcao.
    public string? OrdenarPor { get; set; }
    public bool Decrescente { get; set; }
}
