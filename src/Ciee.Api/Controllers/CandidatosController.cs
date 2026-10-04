using Ciee.Application.DTOs;
using Ciee.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ciee.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CandidatosController : ControllerBase
{
    private readonly ICandidatoAppService _candidatoAppService;
    private readonly IPdfService _pdfService;

    public CandidatosController(ICandidatoAppService candidatoAppService, IPdfService pdfService)
    {
        _candidatoAppService = candidatoAppService;
        _pdfService = pdfService;
    }

    // POST: api/candidatos (Cadastro Manual ou Final após revisão do PDF)
    [HttpPost]
    public async Task<IActionResult> Cadastrar([FromBody] CadastrarCandidatoRequest request)
    {
        try
        {
            var id = await _candidatoAppService.CadastrarAsync(request);
            return CreatedAtAction(nameof(ObterPorId), new { id }, new { id, mensagem = "Candidato cadastrado com sucesso!" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = "Ocorreu um erro interno ao cadastrar o candidato.", detalhes = ex.Message });
        }
    }

    // GET: api/candidatos (Listagem)
    [HttpGet]
    public async Task<IActionResult> ObterTodos()
    {
        var candidatos = await _candidatoAppService.ObterTodosAsync();
        return Ok(candidatos);
    }

    // GET: api/candidatos/{id} (Detalhes)
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var candidato = await _candidatoAppService.ObterPorIdAsync(id);
        if (candidato == null)
            return NotFound(new { erro = "Candidato não encontrado." });

        return Ok(candidato);
    }

    // POST: api/candidatos/extrair-pdf (Extração de dados do currículo em PDF)
    [HttpPost("extrair-pdf")]
    public async Task<IActionResult> ExtrairPdf(IFormFile arquivo)
    {
        if (arquivo == null || arquivo.Length == 0)
            return BadRequest(new { erro = "Nenhum arquivo foi enviado." });

        // Validação de tamanho: até 5 MB (requisito do desafio)
        const long tamanhoMaximo = 5 * 1024 * 1024;
        if (arquivo.Length > tamanhoMaximo)
            return BadRequest(new { erro = "O arquivo excede o limite máximo permitido de 5 MB." });

        // Validação de extensão/tipo
        if (!arquivo.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) &&
            !arquivo.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { erro = "O arquivo enviado não é um PDF válido." });
        }

        try
        {
            using var stream = arquivo.OpenReadStream();
            var resultadoExtracao = await _pdfService.ExtrairDadosDoPdfAsync(stream);

            return Ok(resultadoExtracao);
        }
        catch (Exception)
        {
            // Requisito: A falha na leitura não pode impedir o cadastro manual
            // Retornamos um objeto vazio ou parcial com status 200 para o front tratar sem quebrar a tela
            return Ok(new ExtrairPdfResponse
            {
                NomeCompleto = null,
                Email = null,
                Telefone = null,
                ResumoProfissional = "Falha ao ler o texto do PDF automaticamente. Por favor, preencha manualmente."
            });
        }
    }
}