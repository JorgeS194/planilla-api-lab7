# API Planilla

## Base de datos local

1. Abre [database/PlanillaDb.sql](database/PlanillaDb.sql) en SSMS y ejecútalo en tu instancia de SQL Server. El script crea `PlanillaDb` y carga cuatro departamentos y cuatro empleados en una base nueva. Contabilidad tiene dos empleados y Archivo queda vacío.
2. Crea `backend/PlanillaApi/appsettings.Development.json` con la conexión de tu instancia. El archivo está excluido por `.gitignore`. En cadenas JSON, escribe dos barras invertidas para separar el servidor del nombre de la instancia.

   ```json
   {
     "ConnectionStrings": {
       "PlanillaDb": "Server=.\\SQLEXPRESS;Database=PlanillaDb;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

   La configuración base usa la instancia local `.\\SQLEXPRESS`. Si usas otra, escribe su nombre en tu archivo local. Por ejemplo, para LocalDB usa `(localdb)\\MSSQLLocalDB`.

3. Abre [backend/PlanillaApi.slnx](backend/PlanillaApi.slnx) con el SDK de .NET 10 y ejecuta el perfil **https**. Swagger abre en `https://localhost:7001/swagger`.

## Pruebas del laboratorio

- Abre [PlanillaApi.http](backend/PlanillaApi/PlanillaApi.http) en Visual Studio y envía las peticiones en orden sobre una base de prueba. Usa el ID devuelto por el POST de departamentos para actualizar y borrar el departamento creado. Ajusta las variables del archivo si tu base ya tenía datos.
- Contrasta `cantidadEmpleados` con los datos de SQL Server y revisa el encabezado `Location` del POST y el campo `detail` de las respuestas `400` y `409`.
- Después de que se integren los cambios del equipo, repite el checklist con la solución abierta en Visual Studio y verifica en el frontend el CRUD de departamentos, la actualización del selector de empleados y los mensajes de error.

Consulta la [guía de ejecución con capturas](docs/como-ejecutar.md) para iniciar el frontend con Live Server.
