import { ApiPlanilla } from './apiPlanilla.js';

// Dirección de la API (perfil https en launchSettings.json).
const URL_API = 'https://localhost:7001';

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

// ───────── Clase que maneja la página ─────────

class PaginaEmpleados {
    #api;
    #idEnEdicion = null;   // null = creando; un número = editando ese empleado
    #elementos;

    constructor(api) {
        this.#api = api;
        this.#elementos = {
            mensaje: document.getElementById('mensaje'),
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

        await this.#ejecutar(async () => {
            await this.#cargarDepartamentos();
            await this.#cargarEmpleados();
        });
    }

    // ───────── Carga de datos ─────────

    async #cargarDepartamentos() {
        const departamentos = await this.#api.obtenerDepartamentos();
        const select = this.#elementos.formulario.elements.departamentoId;

        select.replaceChildren(new Option('Seleccione un departamento', ''));

        for (const departamento of departamentos) {
            select.add(new Option(departamento.nombre, departamento.id));
        }
    }

    async #cargarEmpleados() {
        const empleados = await this.#api.obtenerEmpleados();
        const cuerpo = this.#elementos.cuerpoTabla;

        cuerpo.replaceChildren();

        if (empleados.length === 0) {
            const fila = document.createElement('tr');
            const celda = this.#crearCelda('No hay empleados registrados.');
            celda.colSpan = 8;
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
            this.#crearCelda(empleado.cedula),
            this.#crearCelda(empleado.nombre),
            this.#crearCelda(empleado.apellido),
            this.#crearCelda(empleado.cargo),
            this.#crearCelda(empleado.nombreDepartamento),
            this.#crearCelda(formatearFecha(empleado.fechaIngreso)),
            this.#crearCelda(formatearMoneda(empleado.salarioBase), 'numero'),
            this.#crearCelda(formatearMoneda(empleado.salarioNeto), 'numero')
        );

        const acciones = document.createElement('td');
        acciones.append(
            this.#crearBoton('Editar', 'boton-secundario', () => this.#editar(empleado.id)),
            this.#crearBoton('Eliminar', 'boton-peligro', () => this.#eliminar(empleado))
        );
        fila.append(acciones);

        return fila;
    }

    #crearCelda(texto, clase = '') {
        const celda = document.createElement('td');
        celda.textContent = texto;   // textContent, nunca innerHTML con datos externos (evita XSS)
        if (clase) {
            celda.className = clase;
        }
        return celda;
    }

    #crearBoton(texto, clase, alHacerClic) {
        const boton = document.createElement('button');
        boton.type = 'button';
        boton.textContent = texto;
        boton.className = clase;
        boton.addEventListener('click', alHacerClic);
        return boton;
    }

    // ───────── Acciones del usuario ─────────

    async #guardar(evento) {
        evento.preventDefault();   // Evita que el formulario recargue la página

        const datos = this.#leerFormulario();
        this.#elementos.botonGuardar.disabled = true;   // Evita doble envío

        await this.#ejecutar(async () => {
            if (this.#idEnEdicion === null) {
                await this.#api.crearEmpleado(datos);
                this.#mostrarMensaje('Empleado creado correctamente.', 'exito');
            } else {
                await this.#api.actualizarEmpleado(this.#idEnEdicion, datos);
                this.#mostrarMensaje('Empleado actualizado correctamente.', 'exito');
            }

            this.#limpiarFormulario();
            await this.#cargarEmpleados();
        });

        this.#elementos.botonGuardar.disabled = false;
    }

    async #editar(id) {
        await this.#ejecutar(async () => {
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
            this.#elementos.tituloFormulario.textContent = `Editar empleado: ${empleado.nombreCompleto}`;
            this.#elementos.botonCancelar.hidden = false;
            campos.cedula.focus();
        });
    }

    async #eliminar(empleado) {
        if (!confirm(`¿Desea eliminar a ${empleado.nombreCompleto}?`)) {
            return;
        }

        await this.#ejecutar(async () => {
            await this.#api.eliminarEmpleado(empleado.id);

            if (this.#idEnEdicion === empleado.id) {
                this.#limpiarFormulario();
            }

            this.#mostrarMensaje('Empleado eliminado correctamente.', 'exito');
            await this.#cargarEmpleados();
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

    // Ejecuta una acción y, si falla, muestra el error en pantalla.
    async #ejecutar(accion) {
        this.#ocultarMensaje();
        try {
            await accion();
        } catch (error) {
            this.#mostrarMensaje(error.message, 'error');
        }
    }

    #mostrarMensaje(texto, tipo) {
        const mensaje = this.#elementos.mensaje;
        mensaje.textContent = texto;
        mensaje.className = `mensaje mensaje-${tipo}`;
        mensaje.hidden = false;
    }

    #ocultarMensaje() {
        this.#elementos.mensaje.hidden = true;
    }
}

// ───────── Punto de entrada ─────────

const pagina = new PaginaEmpleados(new ApiPlanilla(URL_API));
pagina.iniciar();