using Ciee.Domain.Entities;
using Ciee.Domain.Interfaces;
using Ciee.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ciee.Infrastructure.Repositories;

public class CandidatoRepository : ICandidatoRepository
{
    private readonly CieeDbContext _context;

    public CandidatoRepository(CieeDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(Candidato candidato)
    {
        await _context.Candidatos.AddAsync(candidato);
        await _context.SaveChangesAsync();
    }

    async Task<Candidato?> ICandidatoRepository.ObterPorIdAsync(Guid id)
    {
        return await _context.Candidatos.FindAsync(id);
    }

    async Task<IEnumerable<Candidato>> ICandidatoRepository.ObterTodosAsync()
    {
        return await _context.Candidatos
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync();
    }
}