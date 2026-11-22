import { ApiPlanilla } from './apiPlanilla.js';

// Dirección de la API (perfil https en launchSettings.json).
const URL_API = 'https://localhost:9001';

// ───────── Funciones de formato ─────────

const formatoMoneda = new Intl.NumberFormat('es-PA', { style: 'currency', currency: 'USD' });

function formatearMoneda(valor) {
    return formatoMoneda.format(valor);
}

// Convierte "2024-05-20" en "20/05/2024" sin usar Date (evita el desfase por zona horaria).
function formatearFecha(fechaIso) {
    const [anio, mes, dia] = fechaIso.split('-');
    return `${dia}/${mes}/${anio}`;
}

// ───────── Funciones compartidas por las dos secciones ─────────

function crearCelda(texto, clase = '') {
    const celda = document.createElement('td');
    celda.textContent = texto;   // textContent, nunca innerHTML con datos externos (evita XSS)
    if (clase) {
        celda.className = clase;
    }
    return celda;
}

function crearBoton(texto, clase, alHacerClic) {
    const boton = document.createElement('button');
    boton.type = 'button';
    boton.textContent = texto;
    boton.className = clase;
    boton.addEventListener('click', alHacerClic);
    return boton;
}

// ───────── Cuadro de mensajes (único para toda la página) ─────────

class CuadroMensaje {
    #elemento;

    constructor(elemento) {
        this.#elemento = elemento;
    }

    mostrar(texto, tipo) {
        this.#elemento.textContent = texto;
        this.#elemento.className = `mensaje mensaje-${tipo}`;
        this.#elemento.hidden = false;
    }

    ocultar() {
        this.#elemento.hidden = true;
    }

    // Ejecuta una acción y, si falla, muestra el error en pantalla.
    async ejecutar(accion) {
        this.ocultar();
        try {
            await accion();
        } catch (error) {
            this.mostrar(error.message, 'error');
        }
    }
}

// ───────── Sección de empleados ─────────

class PaginaEmpleados {
    #api;
    #mensajes;
    #alCambiar;   // Se llama cuando cambian los empleados (la cantidad por departamento cambia)
    #idEnEdicion = null;   // null = creando; un número = editando ese empleado
    #elementos;

    constructor(api, mensajes, alCambiar) {
        this.#api = api;
        this.#mensajes = mensajes;
        this.#alCambiar = alCambiar;
        this.#elementos = {
            formulario: document.getElementById('formulario-empleado'),
            tituloFormulario: document.getElementById('titulo-formulario'),
            botonGuardar: document.getElementById('boton-guardar'),
            botonCancelar: document.getElementById('boton-cancelar'),
            cuerpoTabla: document.getElementById('cuerpo-tabla')
        };
    }

    async iniciar() {
        this.#elementos.formulario.addEventListener('submit', (evento) => this.#guardar(evento));
        this.#elementos.botonCancelar.addEventListener('click', () => this.#limpiarFormulario());

        await this.#mensajes.ejecutar(() => this.recargarDatos());
    }

    // Vuelve a llenar la lista de departamentos y la tabla de empleados.
    // La sección de departamentos la llama cuando se crea, edita o elimina un departamento.
    async recargarDatos() {
        await this.#cargarDepartamentos();
        await this.#cargarEmpleados();
    }

    // ───────── Carga de datos ─────────

    async #cargarDepartamentos() {
        const departamentos = await this.#api.obtenerDepartamentos();
        const select = this.#elementos.formulario.elements.departamentoId;
        const seleccionado = select.value;   // Se conserva la selección si el departamento sigue existiendo

        select.replaceChildren(new Option('Seleccione un departamento', ''));

        for (const departamento of departamentos) {
            select.add(new Option(departamento.nombre, departamento.id));
        }

        select.value = seleccionado;
    }

    async #cargarEmpleados() {
        const empleados = await this.#api.obtenerEmpleados();
        const cuerpo = this.#elementos.cuerpoTabla;

        cuerpo.replaceChildren();

        if (empleados.length === 0) {
            const fila = document.createElement('tr');
            const celda = crearCelda('No hay empleados registrados.');
            celda.colSpan = 9;
            fila.append(celda);
            cuerpo.append(fila);
            return;
        }

        for (const empleado of empleados) {
            cuerpo.append(this.#crearFila(empleado));
        }
    }

    // ───────── Construcción de la tabla ─────────

    #crearFila(empleado) {
        const fila = document.createElement('tr');

        fila.append(
            crearCelda(empleado.cedula),
            crearCelda(empleado.nombre),
            crearCelda(empleado.apellido),
            crearCelda(empleado.cargo),
            crearCelda(empleado.nombreDepartamento),
            crearCelda(formatearFecha(empleado.fechaIngreso)),
            crearCelda(formatearMoneda(empleado.salarioBase), 'numero'),
            crearCelda(formatearMoneda(empleado.salarioNeto), 'numero')
        );

        const acciones = document.createElement('td');
        acciones.append(
            crearBoton('Editar', 'boton-secundario', () => this.#editar(empleado.id)),
            crearBoton('Eliminar', 'boton-peligro', () => this.#eliminar(empleado))
        );
        fila.append(acciones);

        return fila;
    }

    // ───────── Acciones del usuario ─────────

    async #guardar(evento) {
        evento.preventDefault();   // Evita que el formulario recargue la página

        const datos = this.#leerFormulario();
        this.#elementos.botonGuardar.disabled = true;   // Evita doble envío

        await this.#mensajes.ejecutar(async () => {
            if (this.#idEnEdicion === null) {
                await this.#api.crearEmpleado(datos);
                this.#mensajes.mostrar('Empleado creado correctamente.', 'exito');
            } else {
                await this.#api.actualizarEmpleado(this.#idEnEdicion, datos);
                this.#mensajes.mostrar('Empleado actualizado correctamente.', 'exito');
            }

            this.#limpiarFormulario();
            await this.#cargarEmpleados();
            await this.#alCambiar();
        });

        this.#elementos.botonGuardar.disabled = false;
    }

    async #editar(id) {
        await this.#mensajes.ejecutar(async () => {
            const empleado = await this.#api.obtenerEmpleado(id);
            const campos = this.#elementos.formulario.elements;

            campos.cedula.value = empleado.cedula;
            campos.nombre.value = empleado.nombre;
            campos.apellido.value = empleado.apellido;
            campos.cargo.value = empleado.cargo;
            campos.fechaIngreso.value = empleado.fechaIngreso;
            campos.salarioBase.value = empleado.salarioBase;
            campos.departamentoId.value = empleado.departamentoId;

            this.#idEnEdicion = id;
            this.#elementos.tituloFormulario.textContent = `Editar empleado: ${empleado.nombre} ${empleado.apellido}`;
            this.#elementos.botonCancelar.hidden = false;
            campos.cedula.focus();
        });
    }

    async #eliminar(empleado) {
        if (!confirm(`¿Desea eliminar a ${empleado.nombre} ${empleado.apellido}?`)) {
            return;
        }

        await this.#mensajes.ejecutar(async () => {
            await this.#api.eliminarEmpleado(empleado.id);

            if (this.#idEnEdicion === empleado.id) {
                this.#limpiarFormulario();
            }

            this.#mensajes.mostrar('Empleado eliminado correctamente.', 'exito');
            await this.#cargarEmpleados();
            await this.#alCambiar();
        });
    }

    // ───────── Métodos de apoyo ─────────

    // Arma el objeto que se envía como JSON. Los nombres coinciden con GuardarEmpleadoDto.
    #leerFormulario() {
        const campos = this.#elementos.formulario.elements;

        return {
            cedula: campos.cedula.value.trim(),
            nombre: campos.nombre.value.trim(),
            apellido: campos.apellido.value.trim(),
            cargo: campos.cargo.value.trim(),
            fechaIngreso: campos.fechaIngreso.value || null,
            salarioBase: Number(campos.salarioBase.value),
            departamentoId: Number(campos.departamentoId.value)
        };
    }

    #limpiarFormulario() {
        this.#elementos.formulario.reset();
        this.#idEnEdicion = null;
        this.#elementos.tituloFormulario.textContent = 'Nuevo empleado';
        this.#elementos.botonCancelar.hidden = true;
    }
}

// ───────── Sección de departamentos ─────────

class PaginaDepartamentos {
    #api;
    #mensajes;
    #alCambiar;   // Se llama cuando cambian los departamentos (hay que actualizar la sección de empleados)
    #idEnEdicion = null;   // null = creando; un número = editando ese departamento
    #elementos;

    constructor(api, mensajes, alCambiar) {
        this.#api = api;
        this.#mensajes = mensajes;
        this.#alCambiar = alCambiar;
        this.#elementos = {
            formulario: document.getElementById('formulario-departamento'),
            tituloFormulario: document.getElementById('titulo-formulario-departamento'),
            botonGuardar: document.getElementById('boton-guardar-departamento'),
            botonCancelar: document.getElementById('boton-cancelar-departamento'),
            cuerpoTabla: document.getElementById('cuerpo-tabla-departamentos')
        };
    }

    async iniciar() {
        this.#elementos.formulario.addEventListener('submit', (evento) => this.#guardar(evento));
        this.#elementos.botonCancelar.addEventListener('click', () => this.#limpiarFormulario());

        await this.#mensajes.ejecutar(() => this.recargarDatos());
    }

    // Vuelve a llenar la tabla de departamentos (la usa también la sección de empleados).
    async recargarDatos() {
        const departamentos = await this.#api.obtenerDepartamentos();
        const cuerpo = this.#elementos.cuerpoTabla;

        cuerpo.replaceChildren();

        if (departamentos.length === 0) {
            const fila = document.createElement('tr');
            const celda = crearCelda('No hay departamentos registrados.');
            celda.colSpan = 4;
            fila.append(celda);
            cuerpo.append(fila);
            return;
        }

        for (const departamento of departamentos) {
            cuerpo.append(this.#crearFila(departamento));
        }
    }

    // ───────── Construcción de la tabla ─────────

    #crearFila(departamento) {
        const fila = document.createElement('tr');

        fila.append(
            crearCelda(departamento.nombre),
            crearCelda(departamento.ubicacion ?? '—'),
            crearCelda(departamento.cantidadEmpleados, 'numero')
        );

        const acciones = document.createElement('td');
        acciones.append(
            crearBoton('Editar', 'boton-secundario', () => this.#editar(departamento.id)),
            crearBoton('Eliminar', 'boton-peligro', () => this.#eliminar(departamento))
        );
        fila.append(acciones);

        return fila;
    }

    // ───────── Acciones del usuario ─────────

    async #guardar(evento) {
        evento.preventDefault();   // Evita que el formulario recargue la página

        const datos = this.#leerFormulario();
        this.#elementos.botonGuardar.disabled = true;   // Evita doble envío

        await this.#mensajes.ejecutar(async () => {
            if (this.#idEnEdicion === null) {
                await this.#api.crearDepartamento(datos);
                this.#mensajes.mostrar('Departamento creado correctamente.', 'exito');
            } else {
                await this.#api.actualizarDepartamento(this.#idEnEdicion, datos);
                this.#mensajes.mostrar('Departamento actualizado correctamente.', 'exito');
            }

            this.#limpiarFormulario();
            await this.recargarDatos();
            await this.#alCambiar();
        });

        this.#elementos.botonGuardar.disabled = false;
    }

    async #editar(id) {
        await this.#mensajes.ejecutar(async () => {
            const departamento = await this.#api.obtenerDepartamento(id);
            const campos = this.#elementos.formulario.elements;

            campos.nombre.value = departamento.nombre;
            campos.ubicacion.value = departamento.ubicacion ?? '';

            this.#idEnEdicion = id;
            this.#elementos.tituloFormulario.textContent = `Editar departamento: ${departamento.nombre}`;
            this.#elementos.botonCancelar.hidden = false;
            campos.nombre.focus();
        });
    }

    async #eliminar(departamento) {
        if (!confirm(`¿Desea eliminar el departamento ${departamento.nombre}?`)) {
            return;
        }

        await this.#mensajes.ejecutar(async () => {
            // Si el departamento tiene empleados la API responde 409 y el mensaje se muestra en pantalla.
            await this.#api.eliminarDepartamento(departamento.id);

            if (this.#idEnEdicion === departamento.id) {
                this.#limpiarFormulario();
            }

            this.#mensajes.mostrar('Departamento eliminado correctamente.', 'exito');
            await this.recargarDatos();
            await this.#alCambiar();
        });
    }

    // ───────── Métodos de apoyo ─────────

    // Arma el objeto que se envía como JSON. Los nombres coinciden con GuardarDepartamentoDto.
    #leerFormulario() {
        const campos = this.#elementos.formulario.elements;

        return {
            nombre: campos.nombre.value.trim(),
            ubicacion: campos.ubicacion.value.trim() || null   // La ubicación es opcional
        };
    }

    #limpiarFormulario() {
        this.#elementos.formulario.reset();
        this.#idEnEdicion = null;
        this.#elementos.tituloFormulario.textContent = 'Nuevo departamento';
        this.#elementos.botonCancelar.hidden = true;
    }
}

// ───────── Punto de entrada ─────────

const api = new ApiPlanilla(URL_API);
const mensajes = new CuadroMensaje(document.getElementById('mensaje'));

// Las dos secciones se avisan entre sí cuando sus datos cambian.
const departamentos = new PaginaDepartamentos(api, mensajes, () => empleados.recargarDatos());
const empleados = new PaginaEmpleados(api, mensajes, () => departamentos.recargarDatos());

empleados.iniciar();
departamentos.iniciar();
