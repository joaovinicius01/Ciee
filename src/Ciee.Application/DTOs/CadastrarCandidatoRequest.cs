namespace Ciee.Application.DTOs;

public class CadastrarCandidatoRequest
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string AreaInteresse { get; set; } = string.Empty;
    public string ResumoProfissional { get; set; } = string.Empty;
}