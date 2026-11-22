namespace PlanillaApi.DTOs
{
    public record EmpleadoDto(
    int Id,
    string Cedula,
    string Nombre,
    string Apellido,
    string Cargo,
    DateOnly FechaIngreso,
    decimal SalarioBase,
    decimal DeduccionSeguroSocial,
    decimal DeduccionSeguroEducativo,
    decimal SalarioNeto,
    int DepartamentoId,
    string NombreDepartamento);
}
