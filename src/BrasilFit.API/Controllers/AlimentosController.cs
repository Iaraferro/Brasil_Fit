using BrasilFit.API.DTOs.Alimentos;
using BrasilFit.API.DTOs.Common;
using BrasilFit.Domain.Entities;
using BrasilFit.Infrastructure.Data;
using BrasilFit.Infrastructure.ExternalServices.OpenFoodFacts;
using BrasilFit.Infrastructure.ExternalServices.Taco;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrasilFit.API.Controllers;

[ApiController]
[Route("api/alimentos")]
public class AlimentosController : ControllerBase
{
    private readonly IOpenFoodFactsService _off;
    private readonly ITacoService _taco;
    private readonly AppDbContext _context;

    public AlimentosController(IOpenFoodFactsService off, ITacoService taco, AppDbContext context)
    {
        _off = off;
        _taco = taco;
        _context = context;
    }

    // ENDPOINT PUBLICO #3a - Busca por codigo de barras no OpenFoodFacts.
    [HttpGet("openfoodfacts/codigo/{codigoBarras}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AlimentoExternoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorCodigoBarras(string codigoBarras, CancellationToken ct)
    {
        var produto = await _off.BuscarPorCodigoBarrasAsync(codigoBarras, ct);
        if (produto is null)
            return NotFound(new { mensagem = "Produto nao encontrado no OpenFoodFacts." });

        return Ok(Mapear(produto));
    }

    // ENDPOINT PUBLICO #3b - Busca por nome no OpenFoodFacts (com paginacao padronizada).
    // GET /api/alimentos/openfoodfacts/buscar?termo=arroz&pagina=1&tamanhoPagina=20
    [HttpGet("openfoodfacts/buscar")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginacaoResultadoDto<AlimentoExternoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BuscarPorNome(
        [FromQuery] string termo,
        [FromQuery] PaginacaoQuery query,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return BadRequest(new ProblemDetails { Title = "Parametro invalido.", Detail = "Informe o termo de busca.", Status = 400 });

        var (produtos, total) = await _off.BuscarPorNomeAsync(termo, query.Pagina, query.TamanhoPagina, ct);

        var itens = produtos.Select(Mapear).ToList();
        var resultado = PaginacaoResultadoDto<AlimentoExternoDto>.Criar(itens, query.Pagina, query.TamanhoPagina, total);
        return Ok(resultado);
    }

    private static AlimentoExternoDto Mapear(OffProduct p) => new()
    {
        CodigoBarras = p.Code,
        Nome = p.ProductNamePt ?? p.ProductName ?? "(sem nome)",
        Marca = p.Brands,
        Kcal = p.Nutriments?.EnergyKcal100g,
        Carboidratos = p.Nutriments?.Carbohydrates100g,
        Proteinas = p.Nutriments?.Proteins100g,
        Lipidios = p.Nutriments?.Fat100g,
        ImagemUrl = p.ImageFrontUrl
    };

    // ENDPOINT #4 - Busca por nome na Tabela TACO (UNICAMP) — alimentos brasileiros.
    // GET /api/alimentos/taco/buscar?termo=arroz
    [HttpGet("taco/buscar")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<TacoAlimentoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarTaco([FromQuery] string termo, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return BadRequest(new ProblemDetails { Title = "Parametro invalido.", Detail = "Informe o termo de busca.", Status = 400 });

        var alimentos = await _taco.BuscarPorNomeAsync(termo, ct);
        return Ok(alimentos.Select(a => new TacoAlimentoDto
        {
            Id           = a.Id,
            Nome         = a.Nome,
            Kcal         = a.Kcal,
            Proteinas    = a.Proteinas,
            Carboidratos = a.Carboidratos,
            Lipidios     = a.Lipidios,
            Fibras       = a.FibrasDieteticas,
            Categoria    = a.Categoria?.Nome
        }));
    }

    [HttpPost("importar")]
    [Authorize]
    public async Task<IActionResult> Importar([FromBody] ImportarAlimentoDTO dto, CancellationToken ct)
    {
        var alimento = await _context.Alimentos
            .FirstOrDefaultAsync(a => a.CodigoBarrasExterno == dto.CodigoBarras, ct);

        if (alimento is null)
        {
            alimento = new Alimento
            {
                Nome = dto.Nome,
                Marca = dto.Marca,
                Kcal = dto.Kcal,
                Carboidratos = dto.Carboidratos,
                Proteinas = dto.Proteinas,
                Lipidios = dto.Lipidios,
                CodigoBarrasExterno = dto.CodigoBarras
            };
            _context.Alimentos.Add(alimento);
            await _context.SaveChangesAsync(ct);
        }

        return Ok(new { id = alimento.Id });
    }
}
