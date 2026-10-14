-- =====================================================================
-- FASE 1.4.4.2 — DDL DE SEGURIDAD (VERSIÓN CORREGIDA ÍNDICES Y BOOTSTRAP)
-- =====================================================================
-- OBJETIVO: 
-- Crear el esquema relacional para soportar Autenticación y Autorización
-- basada en Roles (RBAC) y Permisos (PBAC).
-- 
-- REGLA ARQUITECTÓNICA:
-- Este script es puramente estructural. No inserta datos reales.
-- Sigue el principio SQL-FIRST establecido en la arquitectura base.
-- =====================================================================
-- ---------------------------------------------------------------------
-- 1. TABLA: Usuarios
-- PROPÓSITO:
-- Almacena la identidad de la persona o sistema que accede a la API.
-- ---------------------------------------------------------------------
CREATE TABLE dbo.Usuarios (
    -- PK: Identificador único autoincremental para mayor rendimiento en joins y FKs.
    IdUsuario INT IDENTITY(1, 1) NOT NULL,
    -- Username: Usado para el inicio de sesión. 
    -- VARCHAR(50) es suficiente y más óptimo que NVARCHAR si no se usarán caracteres especiales internacionales en el login.
    Username VARCHAR(50) NOT NULL,
    -- PasswordHash: NUNCA en texto plano. 
    -- VARCHAR(255) es holgado para almacenar hashes complejos como BCrypt o Argon2 (usualmente ~60 caracteres).
    PasswordHash VARCHAR(255) NOT NULL,
    -- Email: Útil para recuperación de cuenta o notificaciones.
    Email VARCHAR(100) NOT NULL,
    -- Activo (Soft Delete): 
    -- BIT funciona como booleano (0 o 1). Permite suspender usuarios sin borrar 
    -- su registro, protegiendo así la integridad referencial (historial de ventas, cajas, etc.).
    Activo BIT NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT(1),
    -- Auditoría (Compatibles con ICurrentUserService futuro):
    FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_Usuarios_FechaCreacion DEFAULT(SYSDATETIME()),
    -- CORRECCIÓN BOOTSTRAP:
    -- Permite NULL. El primer usuario administrador no tendrá un creador rastreable por JWT.
    -- Una vez implementada la auditoría automática, todo registro creado mediante la API 
    -- deberá recibir UsuarioCreacion desde ICurrentUserService y el usuario autenticado, 
    -- salvo registros explícitamente definidos como bootstrap/seed inicial.
    UsuarioCreacion VARCHAR(50) NULL,
    FechaModificacion DATETIME2 NULL,
    UsuarioModificacion VARCHAR(50) NULL,
    CONSTRAINT PK_Usuarios PRIMARY KEY (IdUsuario),
    -- UNIQUE: No pueden existir dos usuarios con el mismo nombre de inicio de sesión o correo.
    CONSTRAINT UQ_Usuarios_Username UNIQUE (Username),
    CONSTRAINT UQ_Usuarios_Email UNIQUE (Email)
);
GO -- ---------------------------------------------------------------------
    -- 2. TABLA: Roles
    -- PROPÓSITO:
    -- Agrupador lógico para asignar conjuntos de permisos a un usuario.
    -- EJ: 'Administrador', 'Vendedor'. Los roles son dinámicos, no hardcodeados en C#.
    -- ---------------------------------------------------------------------
    CREATE TABLE dbo.Roles (
        IdRol INT IDENTITY(1, 1) NOT NULL,
        NombreRol VARCHAR(50) NOT NULL,
        Descripcion VARCHAR(255) NULL,
        Activo BIT NOT NULL CONSTRAINT DF_Roles_Activo DEFAULT(1),
        -- Auditoría
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_Roles_FechaCreacion DEFAULT(SYSDATETIME()),
        -- CORRECCIÓN BOOTSTRAP: NULLable por la creación de los roles iniciales del sistema.
        UsuarioCreacion VARCHAR(50) NULL,
        FechaModificacion DATETIME2 NULL,
        UsuarioModificacion VARCHAR(50) NULL,
        CONSTRAINT PK_Roles PRIMARY KEY (IdRol),
        -- UNIQUE: Evita que existan dos roles que se llamen exactamente igual (ej. dos roles "Cajero").
        CONSTRAINT UQ_Roles_NombreRol UNIQUE (NombreRol)
    );
GO -- ---------------------------------------------------------------------
    -- 3. TABLA: UsuariosRoles (Relación N:M)
    -- PROPÓSITO:
    -- Un usuario puede tener varios roles y un rol puede pertenecer a varios usuarios.
    -- ---------------------------------------------------------------------
    CREATE TABLE dbo.UsuariosRoles (
        IdUsuario INT NOT NULL,
        IdRol INT NOT NULL,
        -- PK Compuesta: Un usuario no puede tener asignado el mismo rol dos veces.
        -- Nota: Al ser PK (IdUsuario, IdRol), el índice Clustered ya cubre las búsquedas 
        -- donde IdUsuario es el primer filtro. No es necesario crear un índice adicional para IdUsuario.
        CONSTRAINT PK_UsuariosRoles PRIMARY KEY (IdUsuario, IdRol),
        -- FKs: 
        -- Si se elimina un usuario o un rol, SQL Server impedirá el borrado (NO ACTION) 
        -- para proteger el historial. Se obliga al uso del "Soft Delete" (campo Activo).
        CONSTRAINT FK_UsuariosRoles_Usuarios FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuarios(IdUsuario),
        CONSTRAINT FK_UsuariosRoles_Roles FOREIGN KEY (IdRol) REFERENCES dbo.Roles(IdRol)
    );
GO -- ---------------------------------------------------------------------
    -- 4. TABLA: Permisos
    -- PROPÓSITO:
    -- Capacidades técnicas atómicas (Ej: INVENTARIO_LEER). 
    -- Las 'Policies' de ASP.NET Core leerán este código.
    -- ---------------------------------------------------------------------
    CREATE TABLE dbo.Permisos (
        IdPermiso INT IDENTITY(1, 1) NOT NULL,
        -- CodigoSistema: El string duro que será evaluado en C# (Ej: 'VENTA_CREAR').
        CodigoSistema VARCHAR(50) NOT NULL,
        Nombre VARCHAR(100) NOT NULL,
        Descripcion VARCHAR(255) NULL,
        Activo BIT NOT NULL CONSTRAINT DF_Permisos_Activo DEFAULT(1),
        -- Auditoría
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_Permisos_FechaCreacion DEFAULT(SYSDATETIME()),
        -- CORRECCIÓN BOOTSTRAP: NULLable por los permisos insertados de raíz.
        UsuarioCreacion VARCHAR(50) NULL,
        FechaModificacion DATETIME2 NULL,
        UsuarioModificacion VARCHAR(50) NULL,
        CONSTRAINT PK_Permisos PRIMARY KEY (IdPermiso),
        -- UNIQUE: No pueden existir dos permisos evaluando el mismo código en el sistema.
        CONSTRAINT UQ_Permisos_CodigoSistema UNIQUE (CodigoSistema)
    );
GO -- ---------------------------------------------------------------------
    -- 5. TABLA: RolesPermisos (Relación N:M)
    -- PROPÓSITO:
    -- Asigna múltiples capacidades (Permisos) a un Agrupador (Rol).
    -- ---------------------------------------------------------------------
    CREATE TABLE dbo.RolesPermisos (
        IdRol INT NOT NULL,
        IdPermiso INT NOT NULL,
        -- PK Compuesta: Un rol no puede tener asignado el mismo permiso dos veces.
        -- Nota: Al ser PK (IdRol, IdPermiso), el índice Clustered ya cubre las búsquedas 
        -- donde IdRol es el primer filtro. No es necesario crear un índice adicional para IdRol.
        CONSTRAINT PK_RolesPermisos PRIMARY KEY (IdRol, IdPermiso),
        -- FKs: Evitan eliminar físicamente un permiso que esté siendo usado por un rol.
        CONSTRAINT FK_RolesPermisos_Roles FOREIGN KEY (IdRol) REFERENCES dbo.Roles(IdRol),
        CONSTRAINT FK_RolesPermisos_Permisos FOREIGN KEY (IdPermiso) REFERENCES dbo.Permisos(IdPermiso)
    );
GO -- ---------------------------------------------------------------------
    -- 6. ÍNDICES DE RENDIMIENTO (NONCLUSTERED)
    -- PROPÓSITO:
    -- Mejorar la velocidad de las consultas inversas (segunda columna de la PK compuesta).
    -- ---------------------------------------------------------------------
    -- Mejora la consulta: "Dame todos los usuarios que tienen el IdRol = Y"
    -- (El índice inverso para IdUsuario ya está cubierto por la PK compuesta)
    CREATE NONCLUSTERED INDEX IX_UsuariosRoles_IdRol ON dbo.UsuariosRoles(IdRol);
-- Mejora la consulta: "Dame todos los roles que usan el IdPermiso = Y"
-- (El índice inverso para IdRol ya está cubierto por la PK compuesta)
CREATE NONCLUSTERED INDEX IX_RolesPermisos_IdPermiso ON dbo.RolesPermisos(IdPermiso);
GO