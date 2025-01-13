-- Crear la base de datos
CREATE DATABASE Recepcion;
GO

-- Usar la base de datos recién creada
USE Recepcion;
GO

-- Crear la tabla Pesaje
CREATE TABLE Pesaje (
    Numero INT IDENTITY(1,1) PRIMARY KEY,  -- Campo número como clave primaria con auto-incremento
    Fecha DATE NOT NULL,                   -- Campo de fecha
    Hora TIME(0) NOT NULL,                 -- Campo de hora
    PesoBruto DECIMAL(10,2) NOT NULL,      -- Campo de peso bruto con precisión de 10 dígitos, 2 decimales
    PesoTara DECIMAL(10,2) NOT NULL,       -- Campo de peso tara con precisión de 10 dígitos, 2 decimales
    PesoNeto DECIMAL(10,2) NOT NULL,       -- Campo de peso neto con precisión de 10 dígitos, 2 decimales
    Unidad NVARCHAR(50) NOT NULL           -- Campo de unidad con hasta 50 caracteres
);
GO

-- Crear un inicio de sesión a nivel del servidor
CREATE LOGIN precitrol
WITH PASSWORD = 'Precitrol2024',
     CHECK_EXPIRATION = OFF,   -- Desactivar la caducidad de la contraseña
     CHECK_POLICY = OFF;       -- Desactivar las políticas de complejidad de la contraseña
GO

-- Crear un usuario para la base de datos basado en el inicio de sesión
USE Recepcion;
GO
CREATE USER precitrol FOR LOGIN precitrol;
GO

-- Otorgar permisos al usuario en la base de datos
ALTER ROLE db_owner ADD MEMBER precitrol; -- Dar permisos completos a la base de datos
GO