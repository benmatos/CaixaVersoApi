using CaixaVersoApi.Models;

namespace CaixaVersoApi.Repositories;

/// <summary>
/// Contrato (interface) que define as operações de persistência de usuários.
/// Permite trocar a implementação sem alterar o restante do sistema (inversão de dependência).
/// </summary>
public interface IUsuarioRepository
{
    /// <summary>Persiste um novo usuário e retorna o objeto criado.</summary>
    /// <param name="usuario">Usuário a ser criado.</param>
    Task<Usuario> CriarAsync(Usuario usuario);

    /// <summary>Retorna todos os usuários cadastrados.</summary>
    Task<IEnumerable<Usuario>> ListarAsync();

    /// <summary>Busca um usuário pelo identificador único.</summary>
    /// <param name="id">GUID do usuário.</param>
    /// <returns>O usuário encontrado ou <c>null</c> se não existir.</returns>
    Task<Usuario?> BuscarPorIdAsync(Guid id);

    /// <summary>Busca um usuário pelo e-mail. Usado para verificar duplicidade.</summary>
    /// <param name="email">E-mail a ser pesquisado (ignorando maiúsculas/minúsculas).</param>
    /// <returns>O usuário encontrado ou <c>null</c> se não existir.</returns>
    Task<Usuario?> BuscarPorEmailAsync(string email);

    /// <summary>Atualiza os dados de um usuário existente.</summary>
    /// <param name="usuario">Usuário com os dados atualizados.</param>
    Task AtualizarAsync(Usuario usuario);
}

/// <summary>
/// Implementação em memória do repositório de usuários.
/// Utiliza um <see cref="Dictionary{TKey,TValue}"/> para simular o banco de dados.
/// </summary>
public class UsuarioRepository : IUsuarioRepository
{
    // Dicionário que armazena usuários em memória usando o Id como chave
    private readonly Dictionary<Guid, Usuario> _usuarios = new();

    /// <inheritdoc/>
    public Task<Usuario> CriarAsync(Usuario usuario)
    {
        _usuarios[usuario.Id] = usuario;
        return Task.FromResult(usuario);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<Usuario>> ListarAsync()
    {
        return Task.FromResult<IEnumerable<Usuario>>(_usuarios.Values);
    }

    /// <inheritdoc/>
    public Task<Usuario?> BuscarPorIdAsync(Guid id)
    {
        _usuarios.TryGetValue(id, out var usuario);
        return Task.FromResult(usuario);
    }

    /// <inheritdoc/>
    public Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        var usuario = _usuarios.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(usuario);
    }

    /// <inheritdoc/>
    public Task AtualizarAsync(Usuario usuario)
    {
        if (_usuarios.ContainsKey(usuario.Id))
        {
            _usuarios[usuario.Id] = usuario;
        }
        return Task.CompletedTask;
    }
}
