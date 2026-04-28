using CaixaVersoApi.DTOs;
using CaixaVersoApi.Models;
using CaixaVersoApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CaixaVersoApi.Controllers;

/// <summary>
/// Controller responsável pelo gerenciamento de usuários.
/// Expõe endpoints REST para criar, listar, buscar, atualizar e desativar usuários.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")] // Custom routing (versioning)
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;

    /// <summary>
    /// Injeta o repositório de usuários via injeção de dependência.
    /// </summary>
    /// <param name="usuarioRepository">Implementação do repositório de usuários.</param>
    public UsuariosController(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Cadastra um novo usuário no sistema.
    /// Verifica se o e-mail já está em uso antes de criar.
    /// </summary>
    /// <param name="dto">Dados necessários para criar o usuário.</param>
    /// <returns>Status 201 com o usuário criado, ou 409 se o e-mail já existir.</returns>
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

    /// <summary>
    /// Retorna a lista completa de usuários cadastrados.
    /// </summary>
    /// <returns>Status 200 com a lista de usuários.</returns>
    [HttpGet]
    public async Task<IActionResult> ListarUsuarios()
    {
        var usuarios = await _usuarioRepository.ListarAsync();
        var dtos = usuarios.Select(MapearParaDto);
        return Ok(dtos);
    }

    /// <summary>
    /// Busca um usuário específico pelo seu identificador único.
    /// </summary>
    /// <param name="id">GUID do usuário.</param>
    /// <returns>Status 200 com o usuário, ou 404 se não encontrado.</returns>
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

    /// <summary>
    /// Atualiza o nome e o cargo de um usuário existente.
    /// </summary>
    /// <param name="id">GUID do usuário a ser atualizado.</param>
    /// <param name="dto">Novos dados do usuário.</param>
    /// <returns>Status 200 com o usuário atualizado, ou 404 se não encontrado.</returns>
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

    /// <summary>
    /// Desativa um usuário (exclusão lógica). O registro não é removido do banco.
    /// </summary>
    /// <param name="id">GUID do usuário a ser desativado.</param>
    /// <returns>Status 200 com mensagem de confirmação, ou 404 se não encontrado.</returns>
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
