using Microsoft.AspNetCore.Mvc;
using PlanillaApi.DTOs;
using PlanillaApi.Services;
namespace PlanillaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpleadosController : Controller
    {
        private readonly IEmpleadoService _servicio;

        public EmpleadosController(IEmpleadoService servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        [ProducesResponseType<IReadOnlyList<EmpleadoDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<EmpleadoDto>>> ObtenerTodos()
        {
            return Ok(await _servicio.ObtenerTodosAsync());
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType<EmpleadoDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmpleadoDto>> ObtenerPorId(int id)
        {
            var empleado = await _servicio.ObtenerPorIdAsync(id);
            return empleado is null ? NotFound() : Ok(empleado);
        }

        [HttpPost]
        [ProducesResponseType<EmpleadoDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EmpleadoDto>> Crear(GuardarEmpleadoDto dto)
        {
            var creado = await _servicio.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Actualizar(int id, GuardarEmpleadoDto dto)
        {
            bool actualizado = await _servicio.ActualizarAsync(id, dto);
            return actualizado ? NoContent() : NotFound();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Eliminar(int id)
        {
            bool eliminado = await _servicio.EliminarAsync(id);
            return eliminado ? NoContent() : NotFound();
        }
    }
}
