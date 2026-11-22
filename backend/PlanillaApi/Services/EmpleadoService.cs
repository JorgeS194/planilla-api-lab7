using Microsoft.EntityFrameworkCore;
using PlanillaApi.Data;
using PlanillaApi.DTOs;
using PlanillaApi.Models;
using PlanillaApi.Exceptions;
namespace PlanillaApi.Services
{
    public class EmpleadoService : IEmpleadoService
    {
        private readonly PlanillaDbContext _contexto;

        public EmpleadoService(PlanillaDbContext contexto)
        {
            _contexto = contexto;
        }

        public async Task<IReadOnlyList<EmpleadoDto>> ObtenerTodosAsync()
        {
            var empleados = await _contexto.Empleados
                .AsNoTracking()
                .Include(e => e.Departamento)
                .OrderBy(e => e.Apellido)
                .ThenBy(e => e.Nombre)
                .ToListAsync();

            return empleados.Select(ConvertirADto).ToList();
        }

        public async Task<EmpleadoDto?> ObtenerPorIdAsync(int id)
        {
            var empleado = await _contexto.Empleados
                .AsNoTracking()
                .Include(e => e.Departamento)
                .FirstOrDefaultAsync(e => e.Id == id);

            return empleado is null ? null : ConvertirADto(empleado);
        }

        public async Task<EmpleadoDto> CrearAsync(GuardarEmpleadoDto dto)
        {
            // Al crear no hay que excluir a nadie: ningún empleado existente tiene Id 0.
            await ValidarReglasAsync(dto, idExcluir: 0);

            var empleado = new Empleado();
            CopiarDatos(dto, empleado);

            _contexto.Empleados.Add(empleado);
            await _contexto.SaveChangesAsync();   // Aquí SQL Server asigna el Id

            // Se vuelve a consultar para incluir el nombre del departamento en la respuesta.
            return (await ObtenerPorIdAsync(empleado.Id))!;
        }

        public async Task<bool> ActualizarAsync(int id, GuardarEmpleadoDto dto)
        {
            var empleado = await _contexto.Empleados.FindAsync(id);
            if (empleado is null)
            {
                return false;   // Primero el 404: si el empleado no existe, no se valida nada más
            }

            // Se excluye al propio empleado para que pueda conservar su misma cédula.
            await ValidarReglasAsync(dto, idExcluir: id);

            CopiarDatos(dto, empleado);
            await _contexto.SaveChangesAsync();   // EF detecta qué cambió y genera el UPDATE
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var empleado = await _contexto.Empleados.FindAsync(id);
            if (empleado is null)
            {
                return false;
            }

            _contexto.Empleados.Remove(empleado);
            await _contexto.SaveChangesAsync();
            return true;
        }

        // ───────── Métodos privados de apoyo ─────────

        // Reglas de negocio que comparten Crear y Actualizar.
        // idExcluir: Id del empleado que se está editando (0 si es uno nuevo).
        private async Task ValidarReglasAsync(GuardarEmpleadoDto dto, int idExcluir)
        {
            // Regla 1: el departamento debe existir -> 400
            bool departamentoExiste = await _contexto.Departamentos
                .AnyAsync(d => d.Id == dto.DepartamentoId);

            if (!departamentoExiste)
            {
                throw new SolicitudInvalidaException("El departamento indicado no existe.");
            }

            // Regla 2: la cédula no puede pertenecer a OTRO empleado -> 409
            string cedula = dto.Cedula.Trim();

            bool cedulaRepetida = await _contexto.Empleados
                .AnyAsync(e => e.Cedula == cedula && e.Id != idExcluir);

            if (cedulaRepetida)
            {
                throw new ConflictoException("Ya existe un empleado con esa cédula.");
            }
        }

        // Crear y editar copian los mismos datos: se escribe una sola vez.
        private static void CopiarDatos(GuardarEmpleadoDto dto, Empleado empleado)
        {
            empleado.Cedula = dto.Cedula.Trim();
            empleado.Nombre = dto.Nombre.Trim();
            empleado.Apellido = dto.Apellido.Trim();
            empleado.Cargo = dto.Cargo.Trim();
            empleado.FechaIngreso = dto.FechaIngreso!.Value;   // [Required] garantiza que no es null
            empleado.SalarioBase = dto.SalarioBase;
            empleado.DepartamentoId = dto.DepartamentoId;
        }

        private static EmpleadoDto ConvertirADto(Empleado empleado) => new(
            empleado.Id,
            empleado.Cedula,
            empleado.Nombre,
            empleado.Apellido,
            empleado.Cargo,
            empleado.FechaIngreso,
            empleado.SalarioBase,
            empleado.DeduccionSeguroSocial,
            empleado.DeduccionSeguroEducativo,
            empleado.SalarioNeto,
            empleado.DepartamentoId,
            empleado.Departamento.Nombre);
    }
}
