// Error propio para las respuestas fallidas de la API.
export class ErrorApi extends Error {
    constructor(mensaje, estado) {
        super(mensaje);
        this.name = 'ErrorApi';
        this.estado = estado;   // Código HTTP (0 si no hubo conexión)
    }
}

// Encapsula todas las llamadas a la API de Planilla.
export class ApiPlanilla {
    #urlBase;

    constructor(urlBase) {
        this.#urlBase = urlBase;
    }

    // ───────── Métodos públicos: uno por endpoint ─────────

    obtenerDepartamentos() {
        return this.#solicitar('/api/departamentos');
    }

    obtenerEmpleados() {
        return this.#solicitar('/api/empleados');
    }

    obtenerEmpleado(id) {
        return this.#solicitar(`/api/empleados/${id}`);
    }

    crearEmpleado(datos) {
        return this.#solicitar('/api/empleados', 'POST', datos);
    }

    actualizarEmpleado(id, datos) {
        return this.#solicitar(`/api/empleados/${id}`, 'PUT', datos);
    }

    eliminarEmpleado(id) {
        return this.#solicitar(`/api/empleados/${id}`, 'DELETE');
    }

    // ───────── Métodos privados de apoyo ─────────

    // Hace la petición HTTP y devuelve el JSON de la respuesta.
    async #solicitar(ruta, metodo = 'GET', cuerpo = null) {
        const opciones = {
            method: metodo,
            headers: { Accept: 'application/json' }
        };

        if (cuerpo !== null) {
            opciones.headers['Content-Type'] = 'application/json';
            opciones.body = JSON.stringify(cuerpo);
        }

        let respuesta;
        try {
            respuesta = await fetch(this.#urlBase + ruta, opciones);
        } catch {
            // fetch solo falla aquí si no hubo respuesta: API apagada, puerto incorrecto o CORS.
            throw new ErrorApi('No se pudo conectar con la API. Verifique que esté en ejecución.', 0);
        }

        // 204 No Content: la operación funcionó y no hay cuerpo que leer.
        if (respuesta.status === 204) {
            return null;
        }

        const contenido = await this.#leerJson(respuesta);

        if (!respuesta.ok) {
            throw new ErrorApi(this.#construirMensajeError(respuesta.status, contenido), respuesta.status);
        }

        return contenido;
    }

    async #leerJson(respuesta) {
        const texto = await respuesta.text();
        if (!texto) {
            return null;
        }

        try {
            return JSON.parse(texto);
        } catch {
            return null;   // La respuesta no era JSON
        }
    }

    // Convierte el ProblemDetails de la API en un mensaje para el usuario.
    #construirMensajeError(estado, problema) {
        // 400 de validación: los errores vienen agrupados por campo.
        if (problema?.errors) {
            return Object.values(problema.errors).flat().join(' ');
        }

        if (estado === 404) {
            return 'El registro solicitado no existe.';
        }

        if (estado >= 500) {
            return 'Ocurrió un error en el servidor. Intente de nuevo más tarde.';
        }

        return problema?.detail ?? problema?.title ?? `Error ${estado}.`;
    }
}