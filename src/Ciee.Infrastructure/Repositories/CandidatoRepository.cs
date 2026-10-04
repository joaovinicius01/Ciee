using Ciee.Domain.Entities;
using Ciee.Domain.Interfaces;
using Ciee.Domain.ValueObjects;
using Ciee.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Ciee.Infrastructure.Repositories;

public class CandidatoRepository : ICandidatoRepository
{
    private const int ViolacaoIndiceUnico = 2601;
    private const int ViolacaoConstraintUnica = 2627;

    private readonly CieeDbContext _context;

    public CandidatoRepository(CieeDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteComEmailAsync(Email email)
    {
        return await _context.Candidatos.AnyAsync(c => c.Email == email);
    }

    public async Task AdicionarAsync(Candidato candidato)
    {
        try
        {
            await _context.Candidatos.AddAsync(candidato);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx &&
                                           (sqlEx.Number == ViolacaoIndiceUnico ||
                                            sqlEx.Number == ViolacaoConstraintUnica))
        {
            // Dois cadastros simultâneos com o mesmo e-mail: o índice único do banco barra o segundo.
            // Vira a mesma exceção da checagem prévia, e o controller responde 409.
            throw new InvalidOperationException("Já existe um candidato cadastrado com este e-mail.", ex);
        }
    }

    public async Task<Candidato?> ObterPorIdAsync(Guid id)
    {
        return await _context.Candidatos.FindAsync(id);
    }

    public async Task<IEnumerable<Candidato>> ObterTodosAsync()
    {
        return await _context.Candidatos.ToListAsync();
    }
}