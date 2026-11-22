using PlanillaApi.DTOs;

namespace PlanillaApi.Services
{
    // Contrato del servicio de departamentos.
    public interface IDepartamentoService
    {
        Task<IReadOnlyList<DepartamentoDto>> ObtenerTodosAsync();
        Task<DepartamentoDto?> ObtenerPorIdAsync(int id);
        Task<DepartamentoDto> CrearAsync(GuardarDepartamentoDto dto);
        Task<bool> ActualizarAsync(int id, GuardarDepartamentoDto dto);
        Task<bool> EliminarAsync(int id);
    }
}
