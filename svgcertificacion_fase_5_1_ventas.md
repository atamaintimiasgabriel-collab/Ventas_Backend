# CERTIFICACIÓN FORENSE PERSISTENCIA VENTAS (FASE 5.1.1)

## 1. Auditoría SQL Real (ATAMAIN / MotorrepuestosDB)
Se ejecutó un script forense completo (`Auditoria_Fase5.1.1.sql`) sobre el motor SQL Server asegurando que los objetos construidos en la Fase 5.1 están formalmente registrados y aplican las reglas en caliente. 

## 2. Inventario de Columnas
**`dbo.Ventas`**: 
- `IdVenta` (PK Identity INT)
- `IdEmpresa`, `IdSucursal`, `IdCliente` (INT, NOT NULL)
- `IdPedido` (INT, NULL) - Confirmado para aceptar Ventas Directas de POS.
- `NumeroVenta` (VARCHAR(20), NOT NULL)
- `Origen` (VARCHAR(20), NOT NULL)
- `Estado` (VARCHAR(20), NOT NULL)
- `Moneda` (VARCHAR(3), NOT NULL)
- `Subtotal`, `Impuesto`, `DescuentoTotal`, `Total` (DECIMAL(18,2), NOT NULL)
- `IdUsuarioVendedor` (INT, NOT NULL)
- `RowVersion` (ROWVERSION, NOT NULL)

**`dbo.VentaDetalles`**:
- `IdDetalleVenta` (PK Identity INT)
- `IdVenta`, `IdProducto` (INT, NOT NULL)
- `Cantidad`, `PrecioUnitario`, `CostoUnitario`, `Descuento`, `PorcentajeImpuesto`, `MontoImpuesto`, `Subtotal`, `TotalLinea` (DECIMAL(18,2), NOT NULL)

*Conclusión:* TODAS las columnas fueron verificadas individualmente y los tipos de datos corresponden con exactitud al diseño.

## 3. FKs (Integridad Referencial)
Todas las relaciones (Empresas, Sucursales, Clientes, Pedidos, Productos) han sido verificadas en la base. 
**Regla crítica auditada:** Toda eliminación fue declarada `ON DELETE NO ACTION`, impidiendo alteraciones del histórico de ventas (ej. no se puede borrar un producto si fue vendido).

## 4. Multiempresa y Aislamiento (UNIQUE / FK)
La protección multiempresa para la unicidad fue confirmada. El índice `UQ_Ventas_NumeroVenta_IdEmpresa` exige que la Empresa no repita sus folios de venta, aislando los entornos. La validación transaccional con clientes de otra empresa debe ser interceptada por C# ya que SQL Server demanda Super Keys para FK compuestas; esta delegación está prevista en el Domain Service posterior.

## 5. Pedido → Venta (Idempotencia y Duplicidad)
Se probó la restricción **`UQ_Ventas_IdPedido_IdEmpresa WHERE IdPedido IS NOT NULL`**. 
SQL Server bloqueó satisfactoriamente (Test 10 y Test 14) la inserción de una segunda venta hacia el mismo `IdPedido`, salvaguardando contra dobles clics de cajeros y fallos de timeout.

## 6. Constraints (CHECK)
Pruebas exhaustivas confirmaron que SQL Server impide silenciosamente la corrupción de datos base:
- `Cantidad <= 0` fue RECHAZADO por `CHK_VentaDetalles_Cantidad` (Test 12).
- Precios e importes negativos fueron RECHAZADOS (Test 13).
- Estados no permitidos (`MAGIA`, `INVALIDO`) fueron RECHAZADOS por `CHK_Ventas_Estado` (Test 17).
- Orígenes desconocidos (`HACKER`) fueron RECHAZADOS por `CHK_Ventas_Origen` (Test 18).

## 7. RowVersion y Concurrencia
- `RowVersion` es administrado autónomamente por el motor (Test 19).
- Modificar el registro cambia dinámicamente el valor binario del timestamp (Test 20).
- En EF Core: Propiedad mapeada mediante `.IsRowVersion()`.

## 8. EF Core vs SQL Server
La metadata en `VentaConfiguration` y `VentaDetalleConfiguration` sincroniza el 100% con los metadatos descubiertos en `sys.tables`, incluyendo precisiones `decimal(18,2)` y longitudes estipuladas. 
**No hay migraciones pendientes.** SQL es la única fuente de verdad autorizada hasta el momento.

## 9. Datos Residuales
El bloque forense operó bajo un entorno `BEGIN TRAN ... ROLLBACK`. 
Resultado: 0 datos residuales. Ninguna venta "sucia" alteró el entorno formal.

## 10. Archivos Modificados
- CREADOS: 
  - `Modelos/Venta.cs`, `Modelos/VentaDetalle.cs`
  - `Configuracion/VentaConfiguration.cs`, `Configuracion/VentaDetalleConfiguration.cs`
- MODIFICADOS: 
  - `Datos/ApplicationDbContext.cs` (Unicamente para registrar los DbSet).
- NO TOCADOS: Ningún archivo perteneciente a Pedidos, Inventario o Seguridad.

## 11. Hallazgos y Decisiones Pendientes
- **CostoUnitario:** El snapshot está correctamente asegurado en `dbo.VentaDetalles`.
- **Estado de Pago:** Validado que `EstadoVenta` no asume estados financieros, separando limpiamente la comercialización de la liquidación monetaria.
