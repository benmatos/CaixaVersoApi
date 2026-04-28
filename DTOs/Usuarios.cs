namespace CaixaVersoApi.DTOs;

public class UsuarioDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
    public string? Cargo { get; set; }
}

public class CriarUsuarioDto
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string? Cargo { get; set; }
}

public class AtualizarUsuarioDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Cargo { get; set; }
}
