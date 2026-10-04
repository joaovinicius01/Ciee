using System.Text.RegularExpressions;
using Ciee.Application.DTOs;
using Ciee.Application.Interfaces;
using UglyToad.PdfPig;

namespace Ciee.Infrastructure.Services;

public class PdfPigService : IPdfService
{
    public async Task<ExtrairPdfResponse> ExtrairDadosDoPdfAsync(Stream arquivoStream)
    {
        var response = new ExtrairPdfResponse();
        var textoCompleto = string.Empty;

        // O PdfPig lê o stream de forma síncrona, rodamos num Task se necessário ou direto
        await Task.Run(() =>
        {
            using var documento = PdfDocument.Open(arquivoStream);
            var escritorTexto = new StringWriter();

            foreach (var pagina in documento.GetPages())
            {
                escritorTexto.WriteLine(pagina.Text);
            }

            textoCompleto = escritorTexto.ToString();
        });

        if (string.IsNullOrWhiteSpace(textoCompleto))
            return response;

        // 1. Extrair E-mail via Regex
        var regexEmail = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.IgnoreCase);
        var matchEmail = regexEmail.Match(textoCompleto);
        if (matchEmail.Success)
        {
            response.Email = matchEmail.Value;
        }

        // 2. Extrair Telefone via Regex (padrões comuns brasileiros com DDD)
        var regexTelefone = new Regex(@"(?:\b\d{2}\b\s*)?(?:9\d{4}[-\s]?\d{4}|\d{4}[-\s]?\d{4})", RegexOptions.IgnoreCase);
        var matchTelefone = regexTelefone.Match(textoCompleto);
        if (matchTelefone.Success)
        {
            response.Telefone = matchTelefone.Value.Trim();
        }

        // 3. Resumo profissional básico (pega os primeiros 500 caracteres do texto extraído)
        response.ResumoProfissional = textoCompleto.Length > 500
            ? textoCompleto.Substring(0, 500).Trim()
            : textoCompleto.Trim();

        // O Nome completo deixamos para o usuário preencher ou pegamos a primeira linha se parecer um nome,
        // mas o desafio diz que a IA/extração não precisa ser perfeita e o usuário revisa no front.
        return response;
    }
}