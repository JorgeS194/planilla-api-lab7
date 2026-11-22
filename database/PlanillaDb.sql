IF DB_ID('PlanillaDb') IS NULL
    CREATE DATABASE PlanillaDb;
GO

USE PlanillaDb;
GO

IF OBJECT_ID('dbo.Departamentos') IS NULL
CREATE TABLE dbo.Departamentos (
    Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Departamentos PRIMARY KEY,
    Nombre    NVARCHAR(100) NOT NULL,
    Ubicacion NVARCHAR(100) NULL
);
GO

IF OBJECT_ID('dbo.Empleados') IS NULL
CREATE TABLE dbo.Empleados (
    Id             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Empleados PRIMARY KEY,
    Cedula         NVARCHAR(20)  NOT NULL,
    Nombre         NVARCHAR(60)  NOT NULL,
    Apellido       NVARCHAR(60)  NOT NULL,
    Cargo          NVARCHAR(80)  NOT NULL,
    FechaIngreso   DATE          NOT NULL,
    SalarioBase    DECIMAL(10,2) NOT NULL,
    DepartamentoId INT           NOT NULL,
    CONSTRAINT UQ_Empleados_Cedula UNIQUE (Cedula),
    CONSTRAINT FK_Empleados_Departamentos
        FOREIGN KEY (DepartamentoId) REFERENCES dbo.Departamentos (Id)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Empleados_DepartamentoId')
    CREATE INDEX IX_Empleados_DepartamentoId ON dbo.Empleados (DepartamentoId);
GO

-- Datos de prueba (solo si la tabla está vacía)
IF NOT EXISTS (SELECT 1 FROM dbo.Departamentos)
BEGIN
    INSERT INTO dbo.Departamentos (Nombre, Ubicacion) VALUES
        (N'Contabilidad',     N'Piso 1'),
        (N'Recursos Humanos', N'Piso 2'),
        (N'Tecnología',       N'Piso 3'),
        (N'Archivo',          N'Sótano');   -- queda vacío a propósito (para probar el DELETE)

    DECLARE @conta INT = (SELECT Id FROM dbo.Departamentos WHERE Nombre = N'Contabilidad');
    DECLARE @rrhh  INT = (SELECT Id FROM dbo.Departamentos WHERE Nombre = N'Recursos Humanos');
    DECLARE @tec   INT = (SELECT Id FROM dbo.Departamentos WHERE Nombre = N'Tecnología');

    INSERT INTO dbo.Empleados (Cedula, Nombre, Apellido, Cargo, FechaIngreso, SalarioBase, DepartamentoId) VALUES
        (N'8-123-456', N'Ana',   N'Gómez',    N'Contadora',      '2023-03-01', 1500.00, @conta),
        (N'8-234-567', N'Luis',  N'Pérez',    N'Asistente',      '2024-01-15',  900.00, @conta),
        (N'8-345-678', N'María', N'Rodríguez', N'Analista de RRHH', '2022-07-10', 1300.00, @rrhh),
        (N'8-456-789', N'Carlos', N'Díaz',    N'Desarrollador',  '2021-11-05', 1800.00, @tec);
END
GO