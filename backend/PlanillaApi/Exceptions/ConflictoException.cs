namespace PlanillaApi.Exceptions;

/// <summary>
/// Se lanza cuando la operación choca con el estado actual de los datos.
/// La API la convierte en una respuesta HTTP 409.
/// </summary>
public class ConflictoException : Exception
{
    public ConflictoException(string mensaje) : base(mensaje)
    {
    }
}