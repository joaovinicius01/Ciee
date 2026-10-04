using Ciee.Domain.Entities;
using Ciee.Domain.ValueObjects;

namespace Ciee.Domain.Interfaces;

public interface ICandidatoRepository
{
    Task<bool> ExisteComEmailAsync(Email email);
    Task AdicionarAsync(Candidato candidato);
    Task<Candidato?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Candidato>> ObterTodosAsync();
}