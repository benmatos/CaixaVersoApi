using CaixaVersoApi.DTOs;
using CaixaVersoApi.Models;
using CaixaVersoApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CaixaVersoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")] // Custom routing (versioning)
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuariosController(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CriarUsuario([FromBody] CriarUsuarioDto dto)
    {
        var existente = await _usuarioRepository.BuscarPorEmailAsync(dto.Email);
        if (existente != null)
        {
            return Conflict(new { mensagem = "E-mail já cadastrado." });
        }

        var novoUsuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            Email = dto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            Ativo = true,
            CriadoEm = DateTime.Now,
            Cargo = dto.Cargo
        };

        var criado = await _usuarioRepository.CriarAsync(novoUsuario);

        var responseDto = MapearParaDto(criado);

        return CreatedAtAction(nameof(BuscarPorId), new { id = criado.Id }, responseDto);
    }

    [HttpGet]
    public async Task<IActionResult> ListarUsuarios()
    {
        var usuarios = await _usuarioRepository.ListarAsync();
        var dtos = usuarios.Select(MapearParaDto);
        return Ok(dtos);
    }

    [HttpGet("{id:guid}")] // Route constraints
    public async Task<IActionResult> BuscarPorId(Guid id)
    {
        var usuario = await _usuarioRepository.BuscarPorIdAsync(id);
        if (usuario == null)
        {
            return NotFound(new { mensagem = "Usuário não encontrado." });
        }

        return Ok(MapearParaDto(usuario));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> AtualizarUsuario(Guid id, [FromBody] AtualizarUsuarioDto dto)
    {
        var usuario = await _usuarioRepository.BuscarPorIdAsync(id);
        if (usuario == null)
        {
            return NotFound(new { mensagem = "Usuário não encontrado." });
        }

        usuario.Nome = dto.Nome;
        usuario.Cargo = dto.Cargo;
        usuario.AtualizadoEm = DateTime.Now;

        await _usuarioRepository.AtualizarAsync(usuario);

        return Ok(MapearParaDto(usuario));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DesativarUsuario(Guid id)
    {
        var usuario = await _usuarioRepository.BuscarPorIdAsync(id);
        if (usuario == null)
        {
            return NotFound(new { mensagem = "Usuário não encontrado." });
        }

        usuario.Ativo = false;
        usuario.AtualizadoEm = DateTime.Now;

        await _usuarioRepository.AtualizarAsync(usuario);

        // Can return NoContent, but returning Ok to show the StandardizedResponseFilter
        return Ok(new { mensagem = "Usuário desativado com sucesso." });
    }

    private static UsuarioDto MapearParaDto(Usuario usuario)
    {
        return new UsuarioDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Ativo = usuario.Ativo,
            CriadoEm = usuario.CriadoEm,
            AtualizadoEm = usuario.AtualizadoEm,
            Cargo = usuario.Cargo
        };
    }
}
