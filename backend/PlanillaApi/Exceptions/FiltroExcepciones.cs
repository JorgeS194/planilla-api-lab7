using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PlanillaApi.Exceptions;

/// <summary>
/// Filtro global: atrapa las excepciones de negocio y responde con el código HTTP
/// correcto y el mensaje en el campo "detail" (formato ProblemDetails).
/// </summary>
public class FiltroExcepciones : IExceptionFilter
{
    public void OnException(ExceptionContext contexto)
    {
        int estado;
        string titulo;

        switch (contexto.Exception)
        {
            case ConflictoException:
                estado = StatusCodes.Status409Conflict;
                titulo = "Conflicto";
                break;

            case SolicitudInvalidaException:
                estado = StatusCodes.Status400BadRequest;
                titulo = "Solicitud inválida";
                break;

            default:
                // Cualquier otra excepción no se toca: sigue como error 500.
                return;
        }

        contexto.Result = new ObjectResult(new ProblemDetails
        {
            Status = estado,
            Title = titulo,
            Detail = contexto.Exception.Message
        })
        {
            StatusCode = estado
        };

        contexto.ExceptionHandled = true;
    }
}