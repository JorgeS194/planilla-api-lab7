using System.ComponentModel.DataAnnotations;

namespace PlanillaApi.DTOs
{
    public class GuardarEmpleadoDto
    {
        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [StringLength(20, ErrorMessage = "La cédula no puede exceder 20 caracteres.")]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(60, ErrorMessage = "El nombre no puede exceder 60 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(60, ErrorMessage = "El apellido no puede exceder 60 caracteres.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cargo es obligatorio.")]
        [StringLength(80, ErrorMessage = "El cargo no puede exceder 80 caracteres.")]
        public string Cargo { get; set; } = string.Empty;

        // Nullable a propósito: así [Required] detecta cuando no se envía.
        [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
        public DateOnly? FechaIngreso { get; set; }

        [Range(0.01, 99_999_999.99, ErrorMessage = "El salario base debe ser mayor que cero.")]
        public decimal SalarioBase { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un departamento válido.")]
        public int DepartamentoId { get; set; }
    }
}
