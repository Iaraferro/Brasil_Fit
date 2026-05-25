using BrasilFit.API.DTOs.Endereco;
using BrasilFit.Infrastructure.ExternalServices.ViaCep;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrasilFit.API.Controllers;

[ApiController]
[Route("api/enderecos")]
public class EnderecosController : ControllerBase
{
    private readonly IViaCepService _viaCep;

    public EnderecosController(IViaCepService viaCep) => _viaCep = viaCep;

    // ENDPOINT PUBLICO #2 - Consulta de endereco via ViaCEP.
    [HttpGet("cep/{cep}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EnderecoConsultaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConsultarPorCep(string cep, CancellationToken ct)
    {
        var dados = await _viaCep.BuscarPorCepAsync(cep, ct);
        if (dados is null)
            return NotFound(new { mensagem = "CEP nao encontrado." });

        return Ok(new EnderecoConsultaDto
        {
            Cep = dados.Cep ?? cep,
            Logradouro = dados.Logradouro ?? string.Empty,
            Bairro = dados.Bairro ?? string.Empty,
            Cidade = dados.Localidade ?? string.Empty,
            Uf = dados.Uf ?? string.Empty,
            Complemento = dados.Complemento
        });
    }
}
