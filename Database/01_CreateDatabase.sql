/* =====================================================================
   Proyecto del Curso - Desarrollo de Ambiente Web (ESAN)
   Plataforma P2P de Intercambio de Divisas
   Motor: SQL Server  |  Enfoque: Database First
   ---------------------------------------------------------------------
   Este script crea la base de datos, las tablas con sus relaciones y
   carga datos de ejemplo (dummy) para probar los endpoints de la API.
   ===================================================================== */

-- Crear la base de datos si no existe
IF DB_ID('DivisaP2PDB') IS NULL
BEGIN
    CREATE DATABASE DivisaP2PDB;
END
GO

USE DivisaP2PDB;
GO

/* ---------------------------------------------------------------------
   Limpieza (orden inverso por dependencias de FK) para re-ejecución
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.DisputaEvidencia', 'U')          IS NOT NULL DROP TABLE dbo.DisputaEvidencia;
IF OBJECT_ID('dbo.Disputa', 'U')                   IS NOT NULL DROP TABLE dbo.Disputa;
IF OBJECT_ID('dbo.Calificacion', 'U')              IS NOT NULL DROP TABLE dbo.Calificacion;
IF OBJECT_ID('dbo.HistorialEstadoTransaccion', 'U')IS NOT NULL DROP TABLE dbo.HistorialEstadoTransaccion;
IF OBJECT_ID('dbo.Voucher', 'U')                   IS NOT NULL DROP TABLE dbo.Voucher;
IF OBJECT_ID('dbo.Transaccion', 'U')               IS NOT NULL DROP TABLE dbo.Transaccion;
IF OBJECT_ID('dbo.Oferta', 'U')                    IS NOT NULL DROP TABLE dbo.Oferta;
IF OBJECT_ID('dbo.CuentaBancaria', 'U')            IS NOT NULL DROP TABLE dbo.CuentaBancaria;
IF OBJECT_ID('dbo.Notificacion', 'U')              IS NOT NULL DROP TABLE dbo.Notificacion;
IF OBJECT_ID('dbo.Usuario', 'U')                   IS NOT NULL DROP TABLE dbo.Usuario;
GO

/* ---------------------------------------------------------------------
   Tabla: Usuario  (cubre los roles USU, ETU y ADM)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Usuario
(
    Id                    INT            IDENTITY(1,1) NOT NULL,
    Rol                   NVARCHAR(3)    NOT NULL,          -- USU / ETU / ADM
    Nombres               NVARCHAR(100)  NOT NULL,
    ApellidoPaterno       NVARCHAR(60)   NULL,
    ApellidoMaterno       NVARCHAR(60)   NULL,
    RazonSocial           NVARCHAR(150)  NULL,              -- solo ETU
    Ruc                   NVARCHAR(11)   NULL,              -- solo ETU
    RepresentanteLegal    NVARCHAR(150)  NULL,              -- solo ETU
    Correo                NVARCHAR(120)  NOT NULL,
    PasswordHash          NVARCHAR(200)  NOT NULL,
    TipoDocumento         NVARCHAR(3)    NULL,              -- DNI / CE
    NumeroDocumento       NVARCHAR(12)   NULL,
    Celular               NVARCHAR(20)   NULL,
    Estado                NVARCHAR(25)   NOT NULL,          -- PendienteVerificacion / PendienteAprobacion / Activo / Bloqueado
    CorreoVerificado      BIT            NOT NULL DEFAULT 0,
    CalificacionPromedio  DECIMAL(3,2)   NOT NULL DEFAULT 0,
    OperacionesCompletadas INT           NOT NULL DEFAULT 0,
    IntentosFallidos      INT            NOT NULL DEFAULT 0,
    BloqueadoHasta        DATETIME2      NULL,
    MotivoBloqueo         NVARCHAR(300)  NULL,
    FechaRegistro         DATETIME2      NOT NULL,
    CONSTRAINT PK_Usuario PRIMARY KEY (Id),
    CONSTRAINT UQ_Usuario_Correo UNIQUE (Correo)
);
GO

/* ---------------------------------------------------------------------
   Tabla: CuentaBancaria  (Usuario 1 -> N CuentaBancaria)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.CuentaBancaria
(
    Id              INT           IDENTITY(1,1) NOT NULL,
    UsuarioId       INT           NOT NULL,
    Banco           NVARCHAR(80)  NOT NULL,
    TipoCuenta      NVARCHAR(15)  NOT NULL,         -- Ahorros / Corriente
    Divisa          NVARCHAR(3)   NOT NULL,         -- PEN / USD / EUR
    NumeroCuenta    NVARCHAR(30)  NOT NULL,
    Cci             NVARCHAR(20)  NOT NULL,
    NombreTitular   NVARCHAR(150) NOT NULL,
    EsPredeterminada BIT          NOT NULL DEFAULT 0,
    CONSTRAINT PK_CuentaBancaria PRIMARY KEY (Id),
    CONSTRAINT FK_CuentaBancaria_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES dbo.Usuario (Id) ON DELETE CASCADE
);
GO

/* ---------------------------------------------------------------------
   Tabla: Oferta  (Usuario 1 -> N Oferta, CuentaBancaria 1 -> N Oferta)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Oferta
(
    Id                INT           IDENTITY(1,1) NOT NULL,
    UsuarioId         INT           NOT NULL,
    TipoOperacion     NVARCHAR(10)  NOT NULL,        -- Compra / Venta
    DivisaOrigen      NVARCHAR(3)   NOT NULL,
    DivisaDestino     NVARCHAR(3)   NOT NULL,
    MontoTotal        DECIMAL(18,2) NOT NULL,
    MontoDisponible   DECIMAL(18,2) NOT NULL,
    TipoCambio        DECIMAL(18,4) NOT NULL,
    Estado            NVARCHAR(15)  NOT NULL,         -- Activa / Expirada / Agotada / Cancelada
    CuentaBancariaId  INT           NOT NULL,
    EsVolumenEtu      BIT           NOT NULL DEFAULT 0,
    FechaPublicacion  DATETIME2     NOT NULL,
    FechaExpiracion   DATETIME2     NOT NULL,
    CONSTRAINT PK_Oferta PRIMARY KEY (Id),
    CONSTRAINT FK_Oferta_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES dbo.Usuario (Id),
    CONSTRAINT FK_Oferta_CuentaBancaria FOREIGN KEY (CuentaBancariaId)
        REFERENCES dbo.CuentaBancaria (Id)
);
GO

/* ---------------------------------------------------------------------
   Tabla: Transaccion
   (Oferta 1 -> N Transaccion; Comprador y Vendedor -> Usuario)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Transaccion
(
    Id                 INT           IDENTITY(1,1) NOT NULL,
    Codigo             NVARCHAR(20)  NOT NULL,        -- TXN-XXXXX
    OfertaId           INT           NOT NULL,
    CompradorId        INT           NOT NULL,
    VendedorId         INT           NOT NULL,
    MontoOperado       DECIMAL(18,2) NOT NULL,
    TipoCambio         DECIMAL(18,4) NOT NULL,
    Estado             NVARCHAR(20)  NOT NULL,
    FechaInicio        DATETIME2     NOT NULL,
    FechaLimiteAccion  DATETIME2     NULL,
    FechaActualizacion DATETIME2     NOT NULL,
    CONSTRAINT PK_Transaccion PRIMARY KEY (Id),
    CONSTRAINT UQ_Transaccion_Codigo UNIQUE (Codigo),
    CONSTRAINT FK_Transaccion_Oferta FOREIGN KEY (OfertaId)
        REFERENCES dbo.Oferta (Id),
    -- Comprador y Vendedor referencian a Usuario sin cascada para evitar
    -- múltiples rutas de borrado en cascada (limitación de SQL Server).
    CONSTRAINT FK_Transaccion_Comprador FOREIGN KEY (CompradorId)
        REFERENCES dbo.Usuario (Id),
    CONSTRAINT FK_Transaccion_Vendedor FOREIGN KEY (VendedorId)
        REFERENCES dbo.Usuario (Id)
);
GO

/* ---------------------------------------------------------------------
   Tabla: Voucher  (Transaccion 1 -> N Voucher)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Voucher
(
    Id              INT           IDENTITY(1,1) NOT NULL,
    TransaccionId   INT           NOT NULL,
    UsuarioId       INT           NOT NULL,
    Tipo            NVARCHAR(10)  NOT NULL,         -- Pago / Entrega
    RutaArchivo     NVARCHAR(300) NOT NULL,
    NombreArchivo   NVARCHAR(200) NOT NULL,
    NumeroOperacion NVARCHAR(50)  NOT NULL,
    FechaDeposito   DATETIME2     NOT NULL,
    FechaSubida     DATETIME2     NOT NULL,
    CONSTRAINT PK_Voucher PRIMARY KEY (Id),
    CONSTRAINT FK_Voucher_Transaccion FOREIGN KEY (TransaccionId)
        REFERENCES dbo.Transaccion (Id) ON DELETE CASCADE
);
GO

/* ---------------------------------------------------------------------
   Tabla: HistorialEstadoTransaccion  (línea de tiempo, US-011)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.HistorialEstadoTransaccion
(
    Id              INT           IDENTITY(1,1) NOT NULL,
    TransaccionId   INT           NOT NULL,
    Estado          NVARCHAR(20)  NOT NULL,
    Comentario      NVARCHAR(300) NULL,
    Fecha           DATETIME2     NOT NULL,
    CONSTRAINT PK_HistorialEstadoTransaccion PRIMARY KEY (Id),
    CONSTRAINT FK_Historial_Transaccion FOREIGN KEY (TransaccionId)
        REFERENCES dbo.Transaccion (Id) ON DELETE CASCADE
);
GO

/* ---------------------------------------------------------------------
   Tabla: Calificacion  (US-012)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Calificacion
(
    Id              INT           IDENTITY(1,1) NOT NULL,
    TransaccionId   INT           NOT NULL,
    CalificadorId   INT           NOT NULL,
    CalificadoId    INT           NOT NULL,
    Estrellas       INT           NOT NULL,
    Comentario      NVARCHAR(200) NULL,
    Fecha           DATETIME2     NOT NULL,
    CONSTRAINT PK_Calificacion PRIMARY KEY (Id),
    -- Un usuario solo puede calificar una vez por transacción (US-012).
    CONSTRAINT UQ_Calificacion_Trans_Calif UNIQUE (TransaccionId, CalificadorId),
    CONSTRAINT FK_Calificacion_Transaccion FOREIGN KEY (TransaccionId)
        REFERENCES dbo.Transaccion (Id) ON DELETE CASCADE
);
GO

/* ---------------------------------------------------------------------
   Tabla: Disputa  (Transaccion 1 -> 1 Disputa, US-014/US-015)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Disputa
(
    Id                   INT            IDENTITY(1,1) NOT NULL,
    TransaccionId        INT            NOT NULL,
    AbiertaPorId         INT            NOT NULL,
    Motivo               NVARCHAR(1000) NOT NULL,
    Estado               NVARCHAR(15)   NOT NULL,        -- Abierta / Resuelta
    Resolucion           NVARCHAR(20)   NULL,            -- AFavorComprador / AFavorVendedor / Anulada
    ComentarioResolucion NVARCHAR(1000) NULL,
    FechaApertura        DATETIME2      NOT NULL,
    FechaResolucion      DATETIME2      NULL,
    CONSTRAINT PK_Disputa PRIMARY KEY (Id),
    CONSTRAINT UQ_Disputa_Transaccion UNIQUE (TransaccionId),
    CONSTRAINT FK_Disputa_Transaccion FOREIGN KEY (TransaccionId)
        REFERENCES dbo.Transaccion (Id) ON DELETE CASCADE
);
GO

/* ---------------------------------------------------------------------
   Tabla: DisputaEvidencia  (Disputa 1 -> N Evidencia)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.DisputaEvidencia
(
    Id            INT           IDENTITY(1,1) NOT NULL,
    DisputaId     INT           NOT NULL,
    RutaArchivo   NVARCHAR(300) NOT NULL,
    NombreArchivo NVARCHAR(200) NOT NULL,
    CONSTRAINT PK_DisputaEvidencia PRIMARY KEY (Id),
    CONSTRAINT FK_DisputaEvidencia_Disputa FOREIGN KEY (DisputaId)
        REFERENCES dbo.Disputa (Id) ON DELETE CASCADE
);
GO

/* ---------------------------------------------------------------------
   Tabla: Notificacion  (Usuario 1 -> N Notificacion, US-021)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Notificacion
(
    Id            INT           IDENTITY(1,1) NOT NULL,
    UsuarioId     INT           NOT NULL,
    Titulo        NVARCHAR(150) NOT NULL,
    Descripcion   NVARCHAR(500) NOT NULL,
    Enlace        NVARCHAR(300) NULL,
    Leida         BIT           NOT NULL DEFAULT 0,
    Fecha         DATETIME2     NOT NULL,
    CONSTRAINT PK_Notificacion PRIMARY KEY (Id),
    CONSTRAINT FK_Notificacion_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES dbo.Usuario (Id) ON DELETE CASCADE
);
GO

/* =====================================================================
   DATOS DE EJEMPLO (DUMMY DATA)
   ---------------------------------------------------------------------
   Contraseña en texto plano de TODOS los usuarios de ejemplo: Password1
   El hash almacenado es un hash BCrypt válido de "Password1".
   ===================================================================== */

DECLARE @hash NVARCHAR(200) = '$2a$11$O6BGnnbm0DjZ9qBqDstKpey0M2tfus7MOfxihA3n2gDM32Rff4jY2';
-- Nota: el AuthService valida con BCrypt.Verify; este hash corresponde a "Password1".

/* ---------- Usuarios ---------- */
SET IDENTITY_INSERT dbo.Usuario ON;
INSERT INTO dbo.Usuario
    (Id, Rol, Nombres, ApellidoPaterno, ApellidoMaterno, RazonSocial, Ruc, RepresentanteLegal,
     Correo, PasswordHash, TipoDocumento, NumeroDocumento, Celular, Estado, CorreoVerificado,
     CalificacionPromedio, OperacionesCompletadas, FechaRegistro)
VALUES
    (1, 'ADM', 'Administrador', 'Plataforma', 'DivisaP2P', NULL, NULL, NULL,
     'admin@divisap2p.com', @hash, NULL, NULL, '999000111', 'Activo', 1, 0, 0, '2026-01-10T09:00:00'),
    (2, 'USU', 'Juan Carlos', 'Pérez', 'Gómez', NULL, NULL, NULL,
     'juan.perez@correo.com', @hash, 'DNI', '40123456', '987654321', 'Activo', 1, 4.50, 3, '2026-02-01T10:00:00'),
    (3, 'USU', 'María Elena', 'Rodríguez', 'Salas', NULL, NULL, NULL,
     'maria.salas@correo.com', @hash, 'DNI', '41987654', '912345678', 'Activo', 1, 5.00, 2, '2026-02-05T11:30:00'),
    (4, 'USU', 'Pedro Luis', 'Vargas', 'Castro', NULL, NULL, NULL,
     'pedro.vargas@correo.com', @hash, 'CE', '001234567', '900111222', 'Activo', 1, 4.00, 1, '2026-02-10T15:00:00'),
    (5, 'ETU', 'Turismo Andino SAC', NULL, NULL, 'Turismo Andino SAC', '20512345678', 'Carlos Fuentes Ríos',
     'ventas@turismoandino.com', @hash, NULL, NULL, '014567890', 'Activo', 1, 4.80, 5, '2026-02-12T09:45:00'),
    (6, 'USU', 'Ana Lucía', 'Torres', 'Mendoza', NULL, NULL, NULL,
     'ana.torres@correo.com', @hash, 'DNI', '42555888', '955666777', 'PendienteVerificacion', 0, 0, 0, '2026-03-01T08:00:00'),
    (7, 'ETU', 'Viajes del Sur EIRL', NULL, NULL, 'Viajes del Sur EIRL', '20687654321', 'Lucía Ramírez Soto',
     'contacto@viajesdelsur.com', @hash, NULL, NULL, '015551234', 'PendienteAprobacion', 0, 0, 0, '2026-03-03T16:20:00');
SET IDENTITY_INSERT dbo.Usuario OFF;
GO

/* ---------- Cuentas bancarias ---------- */
SET IDENTITY_INSERT dbo.CuentaBancaria ON;
INSERT INTO dbo.CuentaBancaria
    (Id, UsuarioId, Banco, TipoCuenta, Divisa, NumeroCuenta, Cci, NombreTitular, EsPredeterminada)
VALUES
    (1, 2, 'BCP',          'Ahorros',   'PEN', '19112345678901', '00219100123456789012', 'Juan Carlos Pérez Gómez', 1),
    (2, 2, 'BCP',          'Ahorros',   'USD', '19198765432101', '00219100876543210198', 'Juan Carlos Pérez Gómez', 1),
    (3, 3, 'Interbank',    'Corriente', 'USD', '20012345678901', '00321200123456789011', 'María Elena Rodríguez Salas', 1),
    (4, 3, 'Interbank',    'Ahorros',   'PEN', '20087654321001', '00321200876543210122', 'María Elena Rodríguez Salas', 1),
    (5, 4, 'BBVA',         'Ahorros',   'EUR', '00112233445566', '01155500112233445566', 'Pedro Luis Vargas Castro', 1),
    (6, 5, 'Scotiabank',   'Corriente', 'USD', '00911223344556', '00977700911223344551', 'Turismo Andino SAC', 1),
    (7, 5, 'Scotiabank',   'Corriente', 'PEN', '00977665544332', '00977700977665544339', 'Turismo Andino SAC', 1);
SET IDENTITY_INSERT dbo.CuentaBancaria OFF;
GO

/* ---------- Ofertas ---------- */
SET IDENTITY_INSERT dbo.Oferta ON;
INSERT INTO dbo.Oferta
    (Id, UsuarioId, TipoOperacion, DivisaOrigen, DivisaDestino, MontoTotal, MontoDisponible,
     TipoCambio, Estado, CuentaBancariaId, EsVolumenEtu, FechaPublicacion, FechaExpiracion)
VALUES
    -- Vende USD a cambio de PEN (tipo de cambio 3.78)
    (1, 2, 'Venta', 'USD', 'PEN', 1000.00, 1000.00, 3.7800, 'Activa', 2, 0, '2026-06-03T08:00:00', '2026-12-31T08:00:00'),
    -- Compra USD pagando con PEN (3.80)
    (2, 3, 'Compra', 'PEN', 'USD',  3800.00, 3800.00, 0.2632, 'Activa', 4, 0, '2026-06-03T09:00:00', '2026-12-31T09:00:00'),
    -- Vende EUR a cambio de PEN (4.10)
    (3, 4, 'Venta', 'EUR', 'PEN',  500.00,  500.00, 4.1000, 'Activa', 5, 0, '2026-06-03T10:00:00', '2026-12-31T10:00:00'),
    -- Oferta en volumen de la ETU: vende USD a cambio de PEN
    (4, 5, 'Venta', 'USD', 'PEN', 50000.00, 50000.00, 3.7650, 'Activa', 6, 1, '2026-06-03T11:00:00', '2026-12-31T11:00:00'),
    -- Oferta ya completada usada para la transacción histórica de ejemplo
    (5, 2, 'Venta', 'USD', 'PEN',  2000.00, 1500.00, 3.7700, 'Activa', 2, 0, '2026-05-20T08:00:00', '2026-12-31T08:00:00');
SET IDENTITY_INSERT dbo.Oferta OFF;
GO

/* ---------- Transacción de ejemplo (completada) ----------
   La oferta 5 (de Juan, vendedor) fue tomada por María (compradora) por 500 USD. */
SET IDENTITY_INSERT dbo.Transaccion ON;
INSERT INTO dbo.Transaccion
    (Id, Codigo, OfertaId, CompradorId, VendedorId, MontoOperado, TipoCambio, Estado,
     FechaInicio, FechaLimiteAccion, FechaActualizacion)
VALUES
    (1, 'TXN-50012', 5, 3, 2, 500.00, 3.7700, 'Completada',
     '2026-05-21T09:30:00', NULL, '2026-05-21T12:00:00');
SET IDENTITY_INSERT dbo.Transaccion OFF;
GO

/* ---------- Historial de la transacción de ejemplo ---------- */
INSERT INTO dbo.HistorialEstadoTransaccion (TransaccionId, Estado, Comentario, Fecha) VALUES
    (1, 'PendientePago',   'Transacción iniciada.',                              '2026-05-21T09:30:00'),
    (1, 'PagoReportado',   'Depósito reportado, pendiente de validación.',       '2026-05-21T09:45:00'),
    (1, 'PagoConfirmado',  'Pago confirmado por el vendedor.',                   '2026-05-21T10:10:00'),
    (1, 'EntregaReportada','Depósito reportado, pendiente de validación.',       '2026-05-21T11:30:00'),
    (1, 'Completada',      'Entrega confirmada por el comprador.',               '2026-05-21T12:00:00');
GO

/* ---------- Vouchers de la transacción de ejemplo ---------- */
INSERT INTO dbo.Voucher (TransaccionId, UsuarioId, Tipo, RutaArchivo, NombreArchivo, NumeroOperacion, FechaDeposito, FechaSubida) VALUES
    (1, 3, 'Pago',    '/uploads/vouchers/txn50012-pago.jpg',    'txn50012-pago.jpg',    'OP-883421', '2026-05-21T09:40:00', '2026-05-21T09:45:00'),
    (1, 2, 'Entrega', '/uploads/vouchers/txn50012-entrega.jpg', 'txn50012-entrega.jpg', 'OP-991233', '2026-05-21T11:20:00', '2026-05-21T11:30:00');
GO

/* ---------- Calificaciones de la transacción de ejemplo ---------- */
INSERT INTO dbo.Calificacion (TransaccionId, CalificadorId, CalificadoId, Estrellas, Comentario, Fecha) VALUES
    (1, 3, 2, 5, 'Excelente, pago rápido y sin problemas.', '2026-05-21T12:30:00'),
    (1, 2, 3, 4, 'Todo correcto.',                          '2026-05-21T13:00:00');
GO

/* ---------- Notificaciones de ejemplo ---------- */
INSERT INTO dbo.Notificacion (UsuarioId, Titulo, Descripcion, Enlace, Leida, Fecha) VALUES
    (2, 'Transacción completada', 'La transacción TXN-50012 se completó exitosamente.', '/transacciones/1', 1, '2026-05-21T12:00:00'),
    (3, 'Transacción completada', 'La transacción TXN-50012 se completó. Ya puedes calificar.', '/transacciones/1', 0, '2026-05-21T12:00:00'),
    (7, 'Empresa en revisión',    'Tu solicitud de registro está pendiente de aprobación.', NULL, 0, '2026-03-03T16:20:00');
GO

PRINT 'Base de datos DivisaP2PDB creada correctamente con datos de ejemplo.';
PRINT 'Usuarios de prueba (contraseña: Password1):';
PRINT '  admin@divisap2p.com (ADM) | juan.perez@correo.com (USU) | maria.salas@correo.com (USU)';
PRINT '  pedro.vargas@correo.com (USU) | ventas@turismoandino.com (ETU)';
GO
