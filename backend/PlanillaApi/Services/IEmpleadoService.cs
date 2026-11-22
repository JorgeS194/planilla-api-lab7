using PlanillaApi.DTOs;

namespace PlanillaApi.Services
{
    public interface IEmpleadoService
    {
        Task<IReadOnlyList<EmpleadoDto>> ObtenerTodosAsync();
        Task<EmpleadoDto?> ObtenerPorIdAsync(int id);
        Task<EmpleadoDto> CrearAsync(GuardarEmpleadoDto dto);
        Task<bool> ActualizarAsync(int id, GuardarEmpleadoDto dto);
        Task<bool> EliminarAsync(int id);
    }
}
