namespace PlanillaApi.DTOs
{
    // Datos que la API devuelve para un departamento.
    public record DepartamentoDto(
        int Id,
        string Nombre,
        string? Ubicacion,
        int CantidadEmpleados);
}
