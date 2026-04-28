using CaixaVersoApi.Models;

namespace CaixaVersoApi.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario> CriarAsync(Usuario usuario);
    Task<IEnumerable<Usuario>> ListarAsync();
    Task<Usuario?> BuscarPorIdAsync(Guid id);
    Task<Usuario?> BuscarPorEmailAsync(string email);
    Task AtualizarAsync(Usuario usuario);
}

public class UsuarioRepository : IUsuarioRepository
{
    private readonly Dictionary<Guid, Usuario> _usuarios = new();

    public Task<Usuario> CriarAsync(Usuario usuario)
    {
        _usuarios[usuario.Id] = usuario;
        return Task.FromResult(usuario);
    }

    public Task<IEnumerable<Usuario>> ListarAsync()
    {
        return Task.FromResult<IEnumerable<Usuario>>(_usuarios.Values);
    }

    public Task<Usuario?> BuscarPorIdAsync(Guid id)
    {
        _usuarios.TryGetValue(id, out var usuario);
        return Task.FromResult(usuario);
    }

    public Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        var usuario = _usuarios.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(usuario);
    }

    public Task AtualizarAsync(Usuario usuario)
    {
        if (_usuarios.ContainsKey(usuario.Id))
        {
            _usuarios[usuario.Id] = usuario;
        }
        return Task.CompletedTask;
    }
}
