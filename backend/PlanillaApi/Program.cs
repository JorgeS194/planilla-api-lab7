using Microsoft.EntityFrameworkCore;
using PlanillaApi.Data;
using PlanillaApi.Services;
using PlanillaApi.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// REGISTRO DE SERVICIOS 

builder.Services.AddControllers(opciones => opciones.Filters.Add<FiltroExcepciones>()); builder.Services.AddOpenApi();

// Respuestas de error con formato estándar (ProblemDetails).
builder.Services.AddProblemDetails();

// El DbContext generado, con la cadena de conexión de appsettings.Development.json.
builder.Services.AddDbContext<PlanillaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PlanillaDb")));

builder.Services.AddScoped<IDepartamentoService, DepartamentoService>();
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
        policy.WithOrigins("http://localhost:5500", "http://127.0.0.1:5500")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// PIPELINE HTTP 

// Captura cualquier excepción no controlada y responde 500 sin exponer detalles internos.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "API Planilla v1"));
}


app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("PermitirFrontend");
app.MapControllers();

app.Run();