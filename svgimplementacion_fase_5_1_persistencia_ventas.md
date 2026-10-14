# IMPLEMENTACIÓN PERSISTENCIA VENTAS / POS (FASE 5.1)

## 1. Estado Inicial
Previo a esta ejecución, la base de datos `MotorrepuestosDB` no poseía las tablas `Ventas` ni `VentaDetalles`.
El ApplicationDbContext solo gestionaba catálogos, empresas, clientes y pedidos.
La base de datos se encontraba en su estructura limpia tras finalizar la fase 4.5.

## 2. Diseño Aprobado Utilizado
Se ejecutó la arquitectura propuesta en `svgdiseno_dominio_ventas_pos.md` donde:
- **Venta:** Entidad estrictamente comercial y cabecera de la operación financiera.
- **VentaDetalle:** Snapshot inmutable de las características comerciales (precios, cantidades, impuestos).
- Se prohíbe relacionar entidades futuras (como facturas o pagos) dentro de estas tablas.

## 3. Tablas Creadas (Vía Script DDL puro)
- `dbo.Ventas`
- `dbo.VentaDetalles`

## 4. Multiempresa y Relaciones
- Implementación de FOREIGN KEYS hacia `dbo.Empresas`, `dbo.Sucursales`, `dbo.Clientes`, `dbo.Pedidos` y `dbo.Productos`.
- Todas las eliminaciones físicas fueron configuradas como `ON DELETE NO ACTION` tanto en el script SQL original como en las dependencias de Entity Framework Core (`DeleteBehavior.NoAction`), previniendo caídas en cascada y alteraciones del histórico transaccional.

## 5. Pedido → Venta
- Se introdujo `IdPedido INT NULL` para soportar tanto flujo directo de POS (NULL) como conversión desde carrito web.
- **UNIQUE Index Filtrado:** `CREATE UNIQUE NONCLUSTERED INDEX UQ_Ventas_IdPedido_IdEmpresa ON dbo.Ventas(IdPedido, IdEmpresa) WHERE IdPedido IS NOT NULL;`. 
- Esta regla SQL pura imposibilita a nivel motor generar una segunda venta para el mismo pedido.

## 6. Idempotencia y Concurrencia
- **Idempotencia:** `IdempotencyKey` protegido por `CREATE UNIQUE NONCLUSTERED INDEX UQ_Ventas_IdEmpresa_Idempotency ON dbo.Ventas(IdEmpresa, IdempotencyKey) WHERE IdempotencyKey IS NOT NULL;`.
- **Concurrencia:** `RowVersion` implementado físicamente como tipo `ROWVERSION` de SQL Server e inyectado a EF Core mediante `.IsRowVersion()`.

## 7. Pruebas Ejecutadas y Resultados Reales
Se ejecutó el banco de pruebas autónomo `Pruebas_Fase5.1_Ventas.sql` sobre la base de datos real con bloques de transacciones volátiles.
- **Test 1:** Inserción Venta válida (PASS)
- **Test 2:** Inserción Detalle válido (PASS)
- **Test 10:** Cantidad <= 0 bloqueada (PASS, detectado `CHK_VentaDetalles_Cantidad`)
- **Test 12:** Estado inválido bloqueado (PASS, detectado `CHK_Ventas_Estado`)
- **Test 13:** Origen inválido bloqueado (PASS, detectado `CHK_Ventas_Origen`)
- **Test 14:** Venta duplicada para mismo Pedido (PASS, detectado `UQ_Ventas_IdPedido_IdEmpresa`)

*Todas las sentencias de inserción fueron revertidas al final del script mediante ROLLBACK TRANSACTION garantizando 0 residuos permanentes en MotorrepuestosDB.*

## 8. EF Core Metadata
- Entidad `Venta` y `VentaDetalle` inyectadas en `ApplicationDbContext`.
- Configuración fluida distribuida en `VentaConfiguration` y `VentaDetalleConfiguration` empleando la interfaz `IEntityTypeConfiguration<T>`.

## 9. Regresión
- Se ejecutó `dotnet build` sobre el proyecto central `SistemaVentasMotorrepuestos.API` con 0 errores y 0 warnings.
- La ejecución del set general de pruebas unitarias sobrepasó correctamente la compilación inicial.

## 10. Riesgos y Siguientes Pasos
- **Fase 5.2:** Con la persistencia estable, el siguiente paso debe orquestar el `VentaService`. Es prioritario crear los contratos DTO antes de tocar la API HTTP para garantizar un Service puro e inviolable.
