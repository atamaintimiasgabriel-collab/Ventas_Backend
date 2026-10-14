# DOCUMENTO MAESTRO: IMPLEMENTACIÓN ETAPA 4.3
## CONCURRENCIA OPTIMISTA + IDEMPOTENCIA AVANZADA EN PEDIDOS

> [!IMPORTANT]
> **Estado:** APROBADO Y APLICADO
> **Dominio:** Pedidos
> **Nivel:** Producción / Pro Ultra

### 1. Modificaciones Estructurales (SQL Server)
Se ejecutó satisfactoriamente el script `Fase4.3_RowVersion_Pedidos.sql` en la base de datos `MotorrepuestosDB` servidor `ATAMAIN`.
* **Columna agregada:** `RowVersion` de tipo `rowversion` (`timestamp`), con restricción `NOT NULL`.
* **Validaciones:** Se comprobó que la tabla ahora cuenta con 24 columnas, manteniendo intactas las restricciones `PK_Pedidos`, `UQ_Pedidos_Numero` y `UQ_Pedidos_IdEmpresa_Idempotency`.

### 2. Modificaciones Entity Framework Core
Se configuró Entity Framework Core para habilitar el manejo de concurrencia optimista utilizando el campo generado por el motor de base de datos.
* **`Pedido.cs`:** Se agregó la propiedad `public byte[] RowVersion { get; set; } = null!;`
* **`PedidoConfiguration.cs`:** Se integró `.IsRowVersion().ValueGeneratedOnAddOrUpdate()` garantizando que EF Core envíe la condición `WHERE RowVersion = @rowVersion` durante las sentencias UPDATE.

### 3. Refactorización de la Capa de Negocio (`PedidoService`)
Se endureció la lógica transaccional de los métodos mutadores, asegurando la atomicidad y controlando excepciones de concurrencia.
* **Firmas de métodos mutadores:** Se actualizó `CambiarEstadoPedidoAsync`, `CancelarPedidoAsync` y `RechazarPedidoAsync` para incluir el parámetro `byte[] rowVersion` y asignar este valor explícitamente a la entidad trakeada.
* **Control de Idempotencia Avanzada:** Se refinó el bloque `catch (DbUpdateException ex)` en `CrearPedidoAsync`. Ahora inspecciona exactamente que la excepción colisione contra la restricción única `UQ_Pedidos_IdEmpresa_Idempotency`, previniendo interceptar colisiones espurias de otras restricciones UNIQUE (ej. `NumeroPedido`) o errores FK. Si la colisión es legítima (ya existe un pedido procesado con el mismo `IdempotencyKey` para la misma empresa), la API recupera y retorna transparente e idempotentemente el recurso ya creado.
* **Detección de Concurrencia (Lost Updates):** Cuando múltiples transacciones intentan alterar el mismo pedido al mismo tiempo, el que llegue en segundo lugar fallará debido al mismatch del `RowVersion`. Esta colisión generará un `DbUpdateConcurrencyException` que es atrapado por el servicio y transformado controladamente en un `PedidoConcurrencyException`.

### 4. Cobertura de Pruebas Unitarias e Integración (Moq / xUnit)
Se modificaron los 57 tests del proyecto `JwtIntegrationTests` para ser compatibles con las nuevas firmas que requieren `byte[] rowVersion`.
* Todos los tests en `PedidoServiceTests.cs` completaron satisfactoriamente y protegen correctamente las reglas de multi-empresa, transiciones de estado, idempotencia pura (colisiones UNIQUE comprobadas en simulación) y ahora el rechazo optimista con `PedidoConcurrencyException`.

---

**Siguiente paso recomendado:**
Validar Etapa 4.3 o avanzar a la siguiente etapa de Análisis y Diseño de los Controladores (Etapa 4.4 - Controllers/Endpoints).
