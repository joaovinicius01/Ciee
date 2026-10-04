using Ciee.Domain.Entities;

namespace Ciee.Domain.Interfaces;

public interface ICandidatoRepository
{
    Task AdicionarAsync(Candidato candidato);
    Task<Candidato?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Candidato>> ObterTodosAsync();
}