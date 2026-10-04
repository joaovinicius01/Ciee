namespace Ciee.Domain.ValueObjects;

public class Email
{
    public string Endereco { get; private set; }

    public Email(string endereco)
    {
        if (string.IsNullOrWhiteSpace(endereco))
            throw new ArgumentException("O e-mail não pode ser vazio.");

        if (endereco.Length > 150)
            throw new ArgumentException("O e-mail deve ter no máximo 150 caracteres.");

        Endereco = endereco;
    }
}