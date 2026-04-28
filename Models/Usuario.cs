namespace CaixaVersoApi.Models;

/// <summary>
/// Representa um usuário da aplicação CaixaVerso.
/// </summary>
public class Usuario
{
    /// <summary>Identificador único do usuário.</summary>
    public Guid Id { get; set; }

    /// <summary>Nome completo do usuário.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Endereço de e-mail do usuário. Utilizado como login.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Hash da senha gerado com BCrypt. Nunca armazene a senha em texto puro.</summary>
    public string SenhaHash { get; set; } = string.Empty;

    /// <summary>Indica se o usuário está ativo no sistema. Exclusão lógica.</summary>
    public bool Ativo { get; set; }

    /// <summary>Data e hora em que o usuário foi cadastrado.</summary>
    public DateTime CriadoEm { get; set; }

    /// <summary>Data e hora da última atualização. <c>null</c> se nunca atualizado.</summary>
    public DateTime? AtualizadoEm { get; set; }

    /// <summary>Cargo ou função do usuário na organização.</summary>
    public string? Cargo { get; set; }
}
