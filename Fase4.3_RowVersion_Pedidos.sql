-- =========================================================================
-- FASE 4.3 - CONCURRENCIA OPTIMISTA EN PEDIDOS
-- =========================================================================
-- Objetivo: Agregar columna RowVersion de tipo rowversion (timestamp)
-- para protección optimista en concurrencia (evitar lost updates).
-- =========================================================================

USE [MotorrepuestosDB];
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Verificar si la tabla existe
    IF OBJECT_ID('dbo.Pedidos', 'U') IS NOT NULL
    BEGIN
        -- 2. Verificar si la columna ya existe
        IF NOT EXISTS (
            SELECT 1 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'Pedidos' 
              AND COLUMN_NAME = 'RowVersion'
        )
        BEGIN
            PRINT 'Agregando columna RowVersion a dbo.Pedidos...';
            
            -- Agregar columna rowversion. En SQL Server 'rowversion' es un sinónimo de 'timestamp'.
            -- No permite NULL nativamente, y se autoincrementa de forma administrada por SQL.
            ALTER TABLE dbo.Pedidos
            ADD [RowVersion] rowversion NOT NULL;
            
            PRINT 'Columna agregada exitosamente.';
        END
        ELSE
        BEGIN
            PRINT 'La columna RowVersion ya existe en dbo.Pedidos. No se realizaron cambios.';
        END
    END
    ELSE
    BEGIN
        PRINT 'ERROR: La tabla dbo.Pedidos no existe.';
    END

    COMMIT TRANSACTION;
    PRINT 'Proceso completado exitosamente.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    PRINT 'Ocurrió un error. Se ha revertido la transacción.';
    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH
GO
