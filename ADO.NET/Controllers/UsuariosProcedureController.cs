using ADO.NET.Models;
using ADO.NET.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ADO.NET.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UsuariosProcedureController : ControllerBase
{
    private readonly IUsuarioProcedureRepository _repository;
    public UsuariosProcedureController(IUsuarioProcedureRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var result = _repository.Get();

        return Ok(result);
    }

    [HttpGet("id")]
    public IActionResult Get([FromQuery] int id)
    {
        var result = _repository.Get(id);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public IActionResult Post([FromBody] Usuario usuario)
    {
        try
        {
            usuario.DataCadastro = DateTimeOffset.Now;
            usuario.SituacaoCadastro = "A";

            _repository.Create(usuario);
        }
        catch (Exception)
        {
            return StatusCode(500, "Erro ao tentar inserir usuário");
        }
        return Ok(usuario);
    }

    [HttpPut]
    public IActionResult Put([FromBody] Usuario usuario)
    {
        _repository.Update(usuario);
        return Ok(usuario);
    }

    [HttpDelete("id")]
    public IActionResult Delete([FromQuery] int id)
    {
        _repository.Delete(id);

        return Ok();
    }
}
