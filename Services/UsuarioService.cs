using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaixaVersoApi.Models;
using CaixaVersoApi.DTOs;

namespace CaixaVersoApi.Services;
public sealed class UsuarioService
{
    private readonly Dictionary<string, Usuario> _db;
    private readonly CriptografiaService _cripto;

    public UsuarioService(Dictionary<string, Usuario> db, CriptografiaService cripto)
    {
        _db = db;
        _cripto = cripto;
    }

    public Task<UsuarioDto> CriarAsync(CriarUsuarioDto dto)
    {
        var emailKey = dto.Email.ToLowerInvariant();
        if (_db.ContainsKey(emailKey)) throw new InvalidOperationException("Email já cadastrado.");

        var u = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            Email = emailKey,
            // SenhaHash você pode manter PBKDF2/BCrypt (não precisa mexer aqui)
            SenhaHash = "hash_aqui",
            DataNascimentoCriptografada = _cripto.Criptografar(dto.DataNascimento.ToString("yyyy-MM-dd")),
            Cargo = dto.Cargo,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _db[emailKey] = u;

        return Task.FromResult(Mapear(u));
    }

    public Task<UsuarioDto?> BuscarPorIdAsync(Guid id)
    {
        var u = _db.Values.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(u is null ? null : Mapear(u));
    }

    private UsuarioDto Mapear(Usuario u)
    {
        var txt = _cripto.Descriptografar(u.DataNascimentoCriptografada);
        var data = DateTime.Parse(txt); // vai sair formatada pelo converter

        return new UsuarioDto
        {
            Id = u.Id,
            Nome = u.Nome,
            Email = u.Email,
            DataNascimento = data,
            Ativo = u.Ativo,
            CriadoEm = u.CriadoEm,
            AtualizadoEm = u.AtualizadoEm,
            Cargo = u.Cargo
        };
    }
}