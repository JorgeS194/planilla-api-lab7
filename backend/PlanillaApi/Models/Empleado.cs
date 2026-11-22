using System;
using System.Collections.Generic;

namespace PlanillaApi.Models;

public partial class Empleado
{
    public int Id { get; set; }

    public string Cedula { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Cargo { get; set; } = null!;

    public DateOnly FechaIngreso { get; set; }

    public decimal SalarioBase { get; set; }

    public int DepartamentoId { get; set; }

    public virtual Departamento Departamento { get; set; } = null!;
}
