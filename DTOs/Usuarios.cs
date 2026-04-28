namespace CaixaVersoApi.DTOs;

/// <summary>
/// Dados do usuário retornados pela API. Nunca expõe a senha.
/// </summary>
public class UsuarioDto
{
    /// <summary>Identificador único do usuário.</summary>
    public Guid Id { get; set; }

    /// <summary>Nome completo do usuário.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Endereço de e-mail do usuário.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Indica se o usuário está ativo no sistema.</summary>
    public bool Ativo { get; set; }

    /// <summary>Data de cadastro do usuário.</summary>
    public DateTime CriadoEm { get; set; }

    /// <summary>Data da última atualização. <c>null</c> se nunca atualizado.</summary>
    public DateTime? AtualizadoEm { get; set; }

    /// <summary>Cargo ou função do usuário.</summary>
    public string? Cargo { get; set; }
}

/// <summary>
/// Dados necessários para cadastrar um novo usuário.
/// </summary>
public class CriarUsuarioDto
{
    /// <summary>Nome completo do usuário. Obrigatório.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>E-mail único do usuário. Usado como login.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Senha em texto puro. Será convertida em hash antes de ser salva.</summary>
    public string Senha { get; set; } = string.Empty;

    /// <summary>Cargo ou função do usuário. Opcional.</summary>
    public string? Cargo { get; set; }
}

/// <summary>
/// Dados permitidos para atualização de um usuário existente.
/// </summary>
public class AtualizarUsuarioDto
{
    /// <summary>Novo nome completo do usuário.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Novo cargo ou função. <c>null</c> para não alterar.</summary>
    public string? Cargo { get; set; }
}
