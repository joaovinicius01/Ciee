using Ciee.Application.DTOs;

namespace Ciee.Application.Interfaces;

public interface IPdfService
{
    Task<ExtrairPdfResponse> ExtrairDadosDoPdfAsync(Stream arquivoStream);
}