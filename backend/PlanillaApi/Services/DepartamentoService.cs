using Microsoft.EntityFrameworkCore;
using PlanillaApi.Data;
using PlanillaApi.DTOs;
namespace PlanillaApi.Services
{
    public class DepartamentoService : IDepartamentoService
    {
        private readonly PlanillaDbContext _contexto;

        public DepartamentoService(PlanillaDbContext contexto)
        {
            _contexto = contexto;
        }

        public async Task<IReadOnlyList<DepartamentoDto>> ObtenerTodosAsync()
        {
            return await _contexto.Departamentos
                .AsNoTracking()
                .OrderBy(d => d.Nombre)
                .Select(d => new DepartamentoDto(d.Id, d.Nombre, d.Ubicacion))
                .ToListAsync();
        }
    }
}
