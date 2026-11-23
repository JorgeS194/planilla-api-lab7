using Microsoft.AspNetCore.Mvc;
using PlanillaApi.DTOs;
using PlanillaApi.Services;

namespace PlanillaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartamentosController : ControllerBase
    {
        private readonly IDepartamentoService _servicio;

        public DepartamentosController(IDepartamentoService servicio)
        {
            _servicio = servicio;
        }

        // GET /api/departamentos
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<DepartamentoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<DepartamentoDto>>> ObtenerTodos()
        {
            return Ok(await _servicio.ObtenerTodosAsync());
        }

        // GET /api/departamentos/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(DepartamentoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DepartamentoDto>> ObtenerPorId(int id)
        {
            var departamento = await _servicio.ObtenerPorIdAsync(id);
            return departamento is null ? NotFound() : Ok(departamento);
        }

        // POST /api/departamentos
        [HttpPost]
        [ProducesResponseType(typeof(DepartamentoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<DepartamentoDto>> Crear(GuardarDepartamentoDto dto)
        {
            var creado = await _servicio.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
        }

        // PUT /api/departamentos/{id}
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Actualizar(int id, GuardarDepartamentoDto dto)
        {
            bool actualizado = await _servicio.ActualizarAsync(id, dto);
            return actualizado ? NoContent() : NotFound();
        }

        // DELETE /api/departamentos/{id}
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Eliminar(int id)
        {
            bool eliminado = await _servicio.EliminarAsync(id);
            return eliminado ? NoContent() : NotFound();
        }
    }
}
