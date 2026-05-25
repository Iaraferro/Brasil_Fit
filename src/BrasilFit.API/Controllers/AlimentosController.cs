using BrasilFit.API.DTOs.Alimentos;
using BrasilFit.Infrastructure.ExternalServices.OpenFoodFacts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrasilFit.API.Controllers;

[ApiController]
[Route("api/alimentos")]
public class AlimentosController : ControllerBase
{
    private readonly IOpenFoodFactsService _off;

    public AlimentosController(IOpenFoodFactsService off) => _off = off;

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

    // ENDPOINT PUBLICO #3b - Busca por nome no OpenFoodFacts.
    [HttpGet("openfoodfacts/buscar")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<AlimentoExternoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarPorNome(
        [FromQuery] string termo,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return BadRequest(new { mensagem = "Informe um termo de busca." });

        var resultados = await _off.BuscarPorNomeAsync(termo, pagina, tamanhoPagina, ct);
        return Ok(resultados.Select(Mapear));
    }

    private static AlimentoExternoDto Mapear(OffProduct p) => new()
    {
        CodigoBarras = p.Code,
        Nome = p.ProductNamePt ?? p.ProductName ?? "(sem nome)",
        Marca = p.Brands,
        Kcal = p.Nutriments?.EnergyKcal100g,
        Carboidratos = p.Nutriments?.Carbohydrates100g,
        Proteinas = p.Nutriments?.Proteins100g,
        Lipidios = p.Nutriments?.Fat100g
    };
}
