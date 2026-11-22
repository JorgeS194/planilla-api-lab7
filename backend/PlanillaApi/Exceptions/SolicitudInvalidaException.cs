namespace PlanillaApi.Exceptions;

/// <summary>
/// Se lanza cuando los datos enviados no son válidos para la regla de negocio
/// (por ejemplo, un departamento que no existe).
/// La API la convierte en una respuesta HTTP 400 (Bad Request).
/// </summary>
public class SolicitudInvalidaException : Exception
{
    public SolicitudInvalidaException(string mensaje) : base(mensaje)
    {
    }
}