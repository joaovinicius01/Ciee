using Ciee.Application.DTOs;
using Ciee.Application.Interfaces;
using Ciee.Domain.Entities;
using Ciee.Domain.Interfaces;
using Ciee.Domain.ValueObjects;

namespace Ciee.Application.Services;

public class CandidatoAppService : ICandidatoAppService
{
    private readonly ICandidatoRepository _candidatoRepository;

    public CandidatoAppService(ICandidatoRepository candidatoRepository)
    {
        _candidatoRepository = candidatoRepository;
    }

    public async Task<Guid> CadastrarAsync(CadastrarCandidatoRequest request)
    {
        var emailVO = new Email(request.Email);

        var jaExiste = await _candidatoRepository.ExisteComEmailAsync(emailVO);
        if (jaExiste)
        {
            throw new InvalidOperationException("Já existe um candidato cadastrado com este e-mail.");
        }

        var candidato = new Candidato(
            request.NomeCompleto,
            emailVO,
            request.Telefone,
            request.AreaInteresse,
            request.ResumoProfissional
        );

        await _candidatoRepository.AdicionarAsync(candidato);
        return candidato.Id;
    }

    public async Task<IEnumerable<CandidatoDto>> ObterTodosAsync()
    {
        var candidatos = await _candidatoRepository.ObterTodosAsync();

        return candidatos.Select(c => new CandidatoDto
        {
            Id = c.Id,
            NomeCompleto = c.NomeCompleto,
            Email = c.Email.Endereco,
            Telefone = c.Telefone,
            AreaInteresse = c.AreaInteresse,
            ResumoProfissional = c.ResumoProfissional,
            DataCriacao = c.DataCriacao
        });
    }

    public async Task<CandidatoDto?> ObterPorIdAsync(Guid id)
    {
        var c = await _candidatoRepository.ObterPorIdAsync(id);
        if (c == null) return null;

        return new CandidatoDto
        {
            Id = c.Id,
            NomeCompleto = c.NomeCompleto,
            Email = c.Email.Endereco,
            Telefone = c.Telefone,
            AreaInteresse = c.AreaInteresse,
            ResumoProfissional = c.ResumoProfissional,
            DataCriacao = c.DataCriacao
        };
    }
}