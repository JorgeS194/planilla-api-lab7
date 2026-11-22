using PlanillaApi.DTOs;
namespace PlanillaApi.Services
{
    public interface IDepartamentoService
    {
        Task<IReadOnlyList<DepartamentoDto>> ObtenerTodosAsync();
    }
}
