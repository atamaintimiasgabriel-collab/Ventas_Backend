# IMPLEMENTACIÓN API REST PEDIDOS (FASE 4.4)

## Estado de la Implementación
**Módulo:** Pedidos (API REST)
**Estado:** Completado
**Validación concurrencia/idempotencia (Fase 0):** Aprobada mediante integración real en BD.

## Arquitectura Implementada

### Controllers (Capa HTTP)
- **`PedidosController`**: Controlador principal mapeado a `/api/v1/pedidos`.
  - Diseñado estrictamente como capa HTTP: 0% lógica de negocio, 100% delegación al `IPedidoService`.
  - Seguridad implementada: `[Authorize]` a nivel de clase, con políticas específicas por endpoint (`PEDIDOS_CREAR`, `PEDIDOS_GESTIONAR`).
  - Endpoint `POST`: Utiliza `Idempotency-Key` opcional desde Headers, retornando 201 Created o 409 Conflict en colisiones idempotentes reales.
  - Endpoints `GET`: Implementan filtros por `PedidoFilterRequest` y ruta `/mis-pedidos` para cliente final (asumiendo inyección segura del Tenant).
  - Endpoint `PATCH`: `/api/v1/pedidos/{id}/estado`. Exige el header `If-Match` para concurrencia optimista, enviando el `RowVersion` serializado a base64. 

### Middleware (Manejo Global de Excepciones)
- Se actualizó el `ManejoExcepcionesMiddleware` para procesar limpiamente el dominio de Pedidos sin exponer el stack trace en HTTP.
  - `PedidoNotFoundException` → `404 Not Found`
  - `PedidoDomainException` → `400 Bad Request` (Transiciones inválidas, datos de negocio erróneos)
  - `PedidoConcurrencyException` → `409 Conflict` (Colisiones RowVersion en actualizaciones)

### Pruebas de Integración (JWT y HttpClient)
- Se desarrolló la batería de pruebas `PedidosControllerTests` asegurando que los endpoints restringen correctamente los recursos (401 Unauthorized sin token).
- Se ejecutó satisfactoriamente `PedidoServiceIntegrationTests` contra SQL Server probando:
  - Idempotencia en la Creación de Pedidos bajo estrés.
  - Colisión Concurrente (Optimista) con `RowVersion`.

## Flujo Final
La arquitectura respeta:
`HTTP Request -> Middleware Auth/Exception -> PedidosController -> [DTO / If-Match Header] -> PedidoService -> PedidoRepository -> EF Core -> SQL Server`
