using CaixaVersoApi.Data;
using CaixaVersoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CaixaVersoApi.Repositories;

public class UsuarioSqlRepository : IUsuarioRepository
{
    private readonly CaixaVersoDbContext _context;

    public UsuarioSqlRepository(CaixaVersoDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario> CriarAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<IEnumerable<Usuario>> ListarAsync()
    {
        return await _context.Usuarios.ToListAsync();
    }

    public async Task<Usuario?> BuscarPorIdAsync(Guid id)
    {
        return await _context.Usuarios.FindAsync(id);
    }

    public async Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task AtualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }
}
