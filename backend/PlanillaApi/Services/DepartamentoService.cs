using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PlanillaApi.Data;
using PlanillaApi.DTOs;
using PlanillaApi.Exceptions;
using PlanillaApi.Models;

namespace PlanillaApi.Services
{
    public class DepartamentoService : IDepartamentoService
    {
        private readonly PlanillaDbContext _contexto;

        public DepartamentoService(PlanillaDbContext contexto)
        {
            _contexto = contexto;
        }

        // GET: listado con cantidad de empleados de cada departamento.
        public async Task<IReadOnlyList<DepartamentoDto>> ObtenerTodosAsync()
        {
            return await _contexto.Departamentos
                .AsNoTracking()
                .OrderBy(d => d.Nombre)
                .Select(d => new DepartamentoDto(
                    d.Id, d.Nombre, d.Ubicacion, d.Empleados.Count))
                .ToListAsync();
        }

        // GET por ID: si no existe, se devuelve null.
        public async Task<DepartamentoDto?> ObtenerPorIdAsync(int id)
        {
            return await _contexto.Departamentos
                .AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new DepartamentoDto(
                    d.Id, d.Nombre, d.Ubicacion, d.Empleados.Count))
                .FirstOrDefaultAsync();
        }

        // POST: crea el departamento y devuelve sus datos con el ID asignado.
        public async Task<DepartamentoDto> CrearAsync(GuardarDepartamentoDto dto)
        {
            var departamento = new Departamento();
            CopiarDatos(dto, departamento);

            _contexto.Departamentos.Add(departamento);
            await _contexto.SaveChangesAsync();

            return new DepartamentoDto(
                departamento.Id, departamento.Nombre, departamento.Ubicacion, 0);
        }

        // PUT: actualiza si existe; si no existe, devuelve false.
        public async Task<bool> ActualizarAsync(int id, GuardarDepartamentoDto dto)
        {
            var departamento = await _contexto.Departamentos.FindAsync(id);
            if (departamento is null)
            {
                return false;
            }

            CopiarDatos(dto, departamento);
            await _contexto.SaveChangesAsync();
            return true;
        }

        // DELETE: no se puede eliminar un departamento que tiene empleados.
        public async Task<bool> EliminarAsync(int id)
        {
            var departamento = await _contexto.Departamentos.FindAsync(id);
            if (departamento is null)
            {
                return false;
            }

            bool tieneEmpleados = await _contexto.Empleados
                .AnyAsync(e => e.DepartamentoId == id);

            if (tieneEmpleados)
            {
                throw new ConflictoException(
                    "No se puede eliminar un departamento que tiene empleados.");
            }

            _contexto.Departamentos.Remove(departamento);

            try
            {
                await _contexto.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (
                ex.InnerException is SqlException sql && sql.Number == 547)
            {
                // También cubre el caso en que se asigne un empleado
                // justo después de revisar si el departamento estaba vacío.
                throw new ConflictoException(
                    "No se puede eliminar un departamento que tiene empleados.");
            }

            return true;
        }

        // Validaciones adicionales para que los datos sean correctos
        // incluso si el servicio es llamado fuera de un controlador.
        private static void CopiarDatos(GuardarDepartamentoDto dto, Departamento departamento)
        {
            if (dto is null)
            {
                throw new SolicitudInvalidaException(
                    "Debe proporcionar los datos del departamento.");
            }

            string nombre = dto.Nombre?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new SolicitudInvalidaException(
                    "El nombre del departamento es obligatorio.");
            }

            if (nombre.Length > 100)
            {
                throw new SolicitudInvalidaException(
                    "El nombre no puede superar los 100 caracteres.");
            }

            string? ubicacion = dto.Ubicacion?.Trim();
            if (ubicacion?.Length > 100)
            {
                throw new SolicitudInvalidaException(
                    "La ubicación no puede superar los 100 caracteres.");
            }

            departamento.Nombre = nombre;
            departamento.Ubicacion = string.IsNullOrWhiteSpace(ubicacion)
                ? null
                : ubicacion;
        }
    }
}
