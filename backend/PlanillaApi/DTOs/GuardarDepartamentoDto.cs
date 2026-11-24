using System.ComponentModel.DataAnnotations;

namespace PlanillaApi.DTOs
{
    // Datos que recibe la API al crear o editar un departamento.
    public class GuardarDepartamentoDto
    {
        [Required(ErrorMessage = "El nombre del departamento es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "La ubicación no puede superar los 100 caracteres.")]
        public string? Ubicacion { get; set; }
    }
}
