using System.ComponentModel.DataAnnotations;

namespace Ciee.Application.DTOs;

public class CadastrarCandidatoRequest
{
    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(150, ErrorMessage = "O nome pode ter no máximo 150 caracteres.")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(150, ErrorMessage = "O e-mail pode ter no máximo 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    // Opcional. Se vier preenchido, precisa ter 10 ou 11 dígitos (DDD + número)
    [RegularExpression(@"^\D*(\d\D*){10,11}$", ErrorMessage = "Informe o telefone com DDD. Exemplo: (41) 99999-9999")]
    [StringLength(20, ErrorMessage = "O telefone pode ter no máximo 20 caracteres.")]
    public string? Telefone { get; set; }

    // Opcional
    [StringLength(100, ErrorMessage = "A área de interesse pode ter no máximo 100 caracteres.")]
    public string? AreaInteresse { get; set; }

    // Opcional
    [StringLength(2000, ErrorMessage = "O resumo pode ter no máximo 2000 caracteres.")]
    public string? ResumoProfissional { get; set; }
}