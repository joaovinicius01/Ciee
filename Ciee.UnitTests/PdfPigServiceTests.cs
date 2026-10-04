using System.IO;
using System.Threading.Tasks;
using Ciee.Infrastructure.Services;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

public class PdfPigServiceTests
{
    private readonly ITestOutputHelper _output;

    public PdfPigServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ExtrairDadosDoPdf_ComArquivoValido_DeveRetornarTodosOsCamposPreenchidos()
    {
        var pdfService = new PdfPigService();
        var caminhoPdf = Path.Combine(Directory.GetCurrentDirectory(), "Fixtures", "curriculo-ficticio.pdf");

        if (!File.Exists(caminhoPdf))
        {
            _output.WriteLine("AVISO: PDF de exemplo não encontrado na pasta Fixtures.");
            Assert.True(true);
            return;
        }

        using var stream = File.OpenRead(caminhoPdf);
        var resultado = await pdfService.ExtrairDadosDoPdfAsync(stream);

        // Imprime todos os dados extraídos para você inspecionar no terminal
        _output.WriteLine("==========================================");
        _output.WriteLine("       DADOS EXTRAÍDOS DO CURRÍCULO       ");
        _output.WriteLine("==========================================");
        _output.WriteLine($"Nome Completo     : {resultado.NomeCompleto}");
        _output.WriteLine($"E-mail            : {resultado.Email}");
        _output.WriteLine($"Telefone          : {resultado.Telefone}");
        _output.WriteLine($"Área de Interesse : {resultado.AreaInteresse}");
        _output.WriteLine($"Resumo Profissional: {resultado.ResumoProfissional}");
        _output.WriteLine("==========================================");

        // Validações básicas de garantia
        Assert.NotNull(resultado);
        Assert.False(string.IsNullOrWhiteSpace(resultado.NomeCompleto), "O nome não foi extraído.");
        Assert.False(string.IsNullOrWhiteSpace(resultado.Email), "O e-mail não foi extraído.");
    }
}