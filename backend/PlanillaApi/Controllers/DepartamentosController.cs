using Microsoft.AspNetCore.Mvc;
using PlanillaApi.DTOs;
using PlanillaApi.Services;
namespace PlanillaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartamentosController : Controller
    {
        private readonly IDepartamentoService _servicio;

        public DepartamentosController(IDepartamentoService servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        [ProducesResponseType<IReadOnlyList<DepartamentoDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<DepartamentoDto>>> ObtenerTodos()
        {
            return Ok(await _servicio.ObtenerTodosAsync());
        }

    }
}
