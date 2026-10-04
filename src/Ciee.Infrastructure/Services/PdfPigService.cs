using System.Globalization;
using System.Text.RegularExpressions;
using Ciee.Application.DTOs;
using Ciee.Application.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Ciee.Infrastructure.Services;

public class PdfPigService : IPdfService
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private static readonly HashSet<string> Preposicoes = new(StringComparer.OrdinalIgnoreCase)
    {
        "da", "de", "do", "das", "dos", "e"
    };

    // Padrões como constantes: usados com Regex.Match/IsMatch estáticos (o .NET faz cache interno)
    private const string PadraoEmail = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";

    // Aceita DDD com ou sem parênteses + número com ou sem hífen/espaço
    private const string PadraoTelefone = @"(?:\(?\b\d{2}\)?[\s\-]*)?(?:9\d{4}|\d{4})[\s\-]?\d{4}\b";

    // Só letras, espaços, acentos, apóstrofo, hífen e ponto
    private const string PadraoNome = @"^[\p{L}\s'\-\.]+$";

    public async Task<ExtrairPdfResponse> ExtrairDadosDoPdfAsync(Stream arquivoStream)
    {
        var response = new ExtrairPdfResponse();
        var linhas = new List<string>();

        await Task.Run(() =>
        {
            using var documento = PdfDocument.Open(arquivoStream);

            foreach (var pagina in documento.GetPages())
            {
                // ContentOrderTextExtractor respeita a ordem visual e insere quebras de linha.
                // (pagina.Text concatena tudo sem separadores, o que quebrava a detecção do nome)
                var textoPagina = ContentOrderTextExtractor.GetText(pagina);
                if (string.IsNullOrWhiteSpace(textoPagina))
                    continue;

                var linhasPagina = textoPagina.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var linha in linhasPagina)
                {
                    var linhaTrim = linha.Trim();
                    if (!string.IsNullOrEmpty(linhaTrim))
                        linhas.Add(linhaTrim);
                }
            }
        });

        if (linhas.Count == 0)
            return response;

        var textoCompleto = string.Join("\n", linhas);

        // Cabeçalho = primeiras linhas, onde ficam nome e contatos
        var cabecalho = string.Join("\n", linhas.Take(10));

        // 1. Nome completo
        response.NomeCompleto = ExtrairNome(linhas);

        // 2. E-mail (prioriza o cabeçalho, fallback para o texto todo)
        var matchEmail = Regex.Match(cabecalho, PadraoEmail, RegexOptions.IgnoreCase);
        if (!matchEmail.Success)
            matchEmail = Regex.Match(textoCompleto, PadraoEmail, RegexOptions.IgnoreCase);
        if (matchEmail.Success)
            response.Email = matchEmail.Value;

        // 3. Telefone (prioriza o cabeçalho, fallback para o texto todo)
        var matchTelefone = Regex.Match(cabecalho, PadraoTelefone);
        if (!matchTelefone.Success)
            matchTelefone = Regex.Match(textoCompleto, PadraoTelefone);
        if (matchTelefone.Success)
            response.Telefone = matchTelefone.Value.Trim();

        // 4. Área de interesse
        response.AreaInteresse = InferirAreaInteresse(textoCompleto);

        // 5. Resumo profissional
        response.ResumoProfissional = ExtrairResumo(linhas);

        return response;
    }

    private static string? ExtrairNome(List<string> linhas)
    {
        foreach (var linha in linhas.Take(5))
        {
            if (linha.Contains('@') || linha.Contains("http", StringComparison.OrdinalIgnoreCase)) continue;
            if (linha.Any(char.IsDigit)) continue;
            if (linha.Length < 5 || linha.Length > 60) continue;

            var palavras = linha.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (palavras.Length < 2 || palavras.Length > 7) continue;

            if (!Regex.IsMatch(linha, PadraoNome)) continue;

            return FormatarNome(linha);
        }

        return null;
    }

    private static string FormatarNome(string nome)
    {
        var textInfo = PtBr.TextInfo;

        var partes = nome
            .ToLower(PtBr)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select((p, i) => i > 0 && Preposicoes.Contains(p) ? p : textInfo.ToTitleCase(p));

        return string.Join(" ", partes);
    }

    private static string InferirAreaInteresse(string texto)
    {
        if (texto.Contains("Back-End", StringComparison.OrdinalIgnoreCase) ||
            texto.Contains(".NET", StringComparison.OrdinalIgnoreCase))
            return "Desenvolvimento Back-End (.NET)";

        if (texto.Contains("Front-End", StringComparison.OrdinalIgnoreCase))
            return "Desenvolvimento Front-End";

        return "Tecnologia da Informação";
    }

    private static string? ExtrairResumo(List<string> linhas)
    {
        // Procura um cabeçalho de seção curto (ex.: "RESUMO PROFISSIONAL", "OBJETIVO", "PERFIL")
        var inicio = linhas.FindIndex(l =>
            l.Length < 40 &&
            (l.Contains("RESUMO", StringComparison.OrdinalIgnoreCase) ||
             l.Contains("OBJETIVO", StringComparison.OrdinalIgnoreCase) ||
             l.Contains("PERFIL", StringComparison.OrdinalIgnoreCase)));

        if (inicio < 0)
            return null;

        var resumo = new List<string>();
        for (var i = inicio + 1; i < linhas.Count; i++)
        {
            var l = linhas[i];

            // Próxima seção: linha curta toda em maiúsculas
            if (l.Length < 40 && l.Any(char.IsLetter) && l == l.ToUpperInvariant())
                break;

            resumo.Add(l);
        }

        var texto = string.Join(" ", resumo).Trim();
        if (string.IsNullOrEmpty(texto))
            return null;

        return texto.Length > 500 ? texto[..500].TrimEnd() + "..." : texto;
    }
}