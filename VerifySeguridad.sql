SET NOCOUNT ON;
PRINT '--- TABLAS ---';
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Usuarios', 'Roles', 'UsuariosRoles', 'Permisos', 'RolesPermisos')
ORDER BY TABLE_NAME;

PRINT '--- COLUMNAS ---';
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('Usuarios', 'Roles', 'UsuariosRoles', 'Permisos', 'RolesPermisos')
  AND COLUMN_NAME IN ('UsuarioCreacion', 'PasswordHash', 'Activo', 'IdUsuario', 'IdRol', 'IdPermiso')
ORDER BY TABLE_NAME, COLUMN_NAME;

PRINT '--- DEFAULT CONSTRAINTS ---';
SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    dc.definition AS DefaultDefinition
FROM sys.tables t
JOIN sys.columns c ON t.object_id = c.object_id
JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
WHERE t.name IN ('Usuarios', 'Roles', 'Permisos')
  AND c.name IN ('Activo', 'FechaCreacion');

PRINT '--- FOREIGN KEYS ---';
SELECT 
    f.name AS ForeignKey,
    OBJECT_NAME(f.parent_object_id) AS TableName,
    delete_referential_action_desc
FROM sys.foreign_keys AS f
WHERE OBJECT_NAME(f.parent_object_id) IN ('UsuariosRoles', 'RolesPermisos');

PRINT '--- INDICES ---';
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique,
    i.is_primary_key
FROM sys.indexes i
JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('UsuariosRoles', 'RolesPermisos', 'Usuarios', 'Roles', 'Permisos')
ORDER BY t.name, i.name;
