# FASE 4: DOMINIO DE PEDIDOS - CIERRE DEFINITIVO

## 1. Estado Final del Dominio Pedidos
**Estado:** COMPLETADO Y CONGELADO.
El dominio ha sido implementado, securizado, optimizado para concurrencia masiva y expuesto mediante API REST.

## 2. Arquitectura Final
- **Web API:** `PedidosController` (Puramente HTTP y orquestador de DTOs).
- **Core Business:** `PedidoService` (Máquina de estados, lógica de cálculo, control concurrente).
- **Acceso a Datos:** `PedidoRepository` y `UnidadDeTrabajo` (Transacciones ACID en SQL Server).
- **Base de Datos:** `MotorrepuestosDB` tabla `Pedidos` y `PedidoDetalles`.

## 3. Endpoints Expuestos
- `POST /api/v1/pedidos` - Creación (K1 Idempotencia).
- `GET /api/v1/pedidos/{id}` - Consulta detallada.
- `GET /api/v1/pedidos` - Consulta paginada administrativa.
- `GET /api/v1/pedidos/mis-pedidos` - Consulta directa para clientes finales.
- `PATCH /api/v1/pedidos/{id}/estado` - Modificación de estados con `If-Match`.

## 4. Autenticación y Tenant Isolation (IDOR)
- **Cero Confianza:** Se asume que el `IdEmpresa` inyectado por HTTP no es válido. Siempre se recupera del JWT.
- **Validación Bidireccional:** Todo Pedido retornado o actualizado verifica `IdEmpresa == _currentUser.IdEmpresa`.

## 5. Idempotencia y Concurrencia
- **Idempotencia:** Constraint `UQ_Pedidos_IdEmpresa_Idempotency` con rescate inteligente (retorna HTTP 200/201 con el Pedido existente si hay colisión limpia).
- **Concurrencia Optimista:** `RowVersion` serializado a ETag (`If-Match`). La API exige este header para cualquier transición de estado. 

## 6. Máquina de Estados
Transiciones blindadas en `PedidoService`:
- PENDIENTE → EN_PROCESO, CANCELADO, RECHAZADO
- EN_PROCESO → CONFIRMADO, CANCELADO, RECHAZADO
- CONFIRMADO → ATENDIDO, CANCELADO, RECHAZADO

## 7. Manejo de Errores (Problem Details)
- Interceptado globalmente en `ManejoExcepcionesMiddleware`.
- Mapeo directo: `PedidoNotFoundException` (404), `PedidoDomainException` (400), `PedidoConcurrencyException` (409). No se filtran Stack Traces a internet.

## 8. Resultados de las Pruebas
- **Pruebas HTTP Reales:** Ejecutadas vía `WebApplicationFactory`. Endpoints securizados con éxito (401/403).
- **Pruebas Concurrencia:** Cajero A vs Cajero B probados en Test Unitario desconectado y Test de Integración SQL, resultando en 1 SUCCESS y 1 `DbUpdateConcurrencyException` (o DomainException).
- **Pruebas Idempotencia:** Ráfaga de +10 tareas concurrentes en SQL generaron exactamente **1 registro**, mapeando las excepciones sobrantes a fallas silenciosas controladas.

## 9. Decisiones Congeladas
- NO se permite `IdEmpresa` por el Body (siempre JWT).
- NO se actualizan datos del Pedido en la fase `PATCH` aparte del estado y motivo.
- NO se guarda "Saldo Actual" en el módulo Clientes, todo se deriva en módulo Pagos (Futuro).

## 10. Dependencias Futuras
El módulo VENTAS tomará como origen de datos la tabla `Pedidos`, asumiendo que un Pedido `CONFIRMADO` puede transformarse en Venta oficial afectando Stock y Cuentas por Cobrar.
