using Ciee.Domain.ValueObjects;

namespace Ciee.Domain.Entities;

public class Candidato : Entity
{
    public string NomeCompleto { get; private set; }
    public Email Email { get; private set; }
    public string Telefone { get; private set; }
    public string AreaInteresse { get; private set; }
    public string ResumoProfissional { get; private set; }
    public DateTime DataCriacao { get; private set; }

    // Construtor para o EF Core mapear corretamente
    protected Candidato() { }

    public Candidato(string nomeCompleto, Email email, string telefone, string areaInteresse, string resumoProfissional)
    {
        Validar(nomeCompleto, email);

        NomeCompleto = nomeCompleto;
        Email = email;
        Telefone = telefone;
        AreaInteresse = areaInteresse;
        ResumoProfissional = resumoProfissional;
        DataCriacao = DateTime.UtcNow;
    }

    private void Validar(string nomeCompleto, Email email)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            throw new ArgumentException("O nome completo é obrigatório.");

        if (email == null)
            throw new ArgumentException("O e-mail é obrigatório.");
    }
}