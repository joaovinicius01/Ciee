using Ciee.Application.DTOs;

namespace Ciee.Application.Interfaces;

public interface ICandidatoAppService
{
    Task<Guid> CadastrarAsync(CadastrarCandidatoRequest request);
    Task<IEnumerable<CandidatoDto>> ObterTodosAsync();
    Task<CandidatoDto?> ObterPorIdAsync(Guid id);
}