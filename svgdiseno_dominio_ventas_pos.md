# FASE 5.0 — DISEÑO TÉCNICO DEL DOMINIO VENTAS / POS

## 1. Objetivo
Diseñar la arquitectura técnica, modelo de datos, flujos de estado y responsabilidades del dominio de Ventas / POS para el sistema `SistemaVentasMotorrepuestos.API`, garantizando aislamiento multiempresa, protección de concurrencia e integración limpia con dominios futuros y pasados (Pedidos, Inventario, Pagos, Caja, Facturación).

## 2. Alcance
- Modelo transaccional de Ventas y Detalles.
- Flujos de orígenes (Venta Directa POS vs Pedido → Venta).
- Estrategias de Idempotencia y Concurrencia.
- Fronteras arquitectónicas con submódulos financieros y de inventario.
- Quedan excluidos de implementación en esta fase: Código, Tablas, Endpoints.

## 3. Venta vs Pedido
- **Pedido:** Intención de compra originada en canales asíncronos (Web/Mobile) o cotizaciones en tienda. No compromete legalmente el movimiento financiero de caja hasta ser procesado.
- **Venta:** Operación comercial formalizada. Representa la entrega/salida de mercadería, el nacimiento de una obligación de pago (o cancelación de la misma) y el origen de comprobantes fiscales.
- **Transformación:** Un `Pedido` (CONFIRMADO) se transforma en `Venta` cuando el cajero/operador autoriza su procesamiento en el POS. Un `Pedido` origina como máximo **una única** `Venta`.

## 4. Modelo Conceptual
```mermaid
graph TD
    WEB[Pedido Web/Mobile] -->|Notifica| POS[Desktop POS]
    POS_DIRECTO[Venta Directa] --> POS
    POS -->|Procesa| VENTA[Dominio Venta]
    VENTA -->|Notifica (ACID)| STOCK[Inventario/Stock]
    VENTA -->|Genera Obligación| PAGOS[Pagos/Créditos]
    PAGOS -->|Refleja| CAJA[Caja Sucursal]
    VENTA -->|Fiscaliza| COMPROBANTE[Facturación/SUNAT]
```

## 5. Tablas Propuestas
- **`dbo.Ventas`**: Cabecera transaccional.
- **`dbo.VentaDetalles`**: Líneas de venta con *snapshot* estático de precios y costos.

No se requieren tablas intermedias adicionales para la esencia de la venta; el estado del pago o facturación se rastreará mediante ID Venta en sus respectivos módulos.

## 6. Relaciones y Campos

### `dbo.Ventas`
| Campo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `IdVenta` | INT | SÍ | PK Identity |
| `IdEmpresa` | INT | SÍ | Multiempresa (FK) |
| `IdSucursal` | INT | SÍ | Sucursal donde ocurre la Venta (FK) |
| `IdCliente` | INT | SÍ | Puede ser "Cliente Genérico" (FK) |
| `IdPedido` | INT | NO | Origen si viene de un Pedido (FK, UNIQUE por Empresa) |
| `NumeroVenta` | VARCHAR | SÍ | Folio comercial interno |
| `Origen` | VARCHAR | SÍ | "POS", "WEB", "MOBILE" |
| `Estado` | VARCHAR | SÍ | Estado transaccional interno |
| `Moneda` | VARCHAR | SÍ | "PEN", "USD" |
| `Subtotal`, `Impuesto`, `DescuentoTotal`, `Total` | DECIMAL | SÍ | Importes consolidados |
| `FechaVenta` | DATETIME2 | SÍ | Fecha comercial |
| `FechaProcesamiento` | DATETIME2 | NO | Fecha real de confirmación/cierre |
| `IdUsuarioVendedor` | INT | SÍ | Vendedor asociado/Cajero |
| `Observaciones` | VARCHAR | NO | Notas |
| `IdempotencyKey` | VARCHAR | NO | UNIQUE Idempotencia de API |
| `RowVersion` | TIMESTAMP | SÍ | Concurrencia Optimista |

### `dbo.VentaDetalles`
| Campo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `IdDetalleVenta` | INT | SÍ | PK Identity |
| `IdVenta` | INT | SÍ | FK Venta |
| `IdProducto` | INT | SÍ | FK Producto |
| `Cantidad` | DECIMAL | SÍ | Cantidad vendida |
| `PrecioUnitario` | DECIMAL | SÍ | *Snapshot* del precio al momento de venta |
| `CostoUnitario` | DECIMAL | SÍ | *Snapshot* del costo (para margen) |
| `Descuento` | DECIMAL | SÍ | Descuento aplicado en la línea |
| `PorcentajeImpuesto` | DECIMAL | SÍ | Tasa IGV |
| `MontoImpuesto` | DECIMAL | SÍ | Calculado |
| `Subtotal`, `TotalLinea` | DECIMAL | SÍ | Importes calculados |

## 7. Máquina de Estados (Ventas)
Estados sugeridos:
1. **`BORRADOR`** (Temporal/POS, opcional si el POS envía todo al final).
2. **`COMPLETADA`** (Venta consolidada, stock descontado).
3. **`ANULADA`** (Venta revertida por error operativo/cajero antes de cierre).
4. **`DEVUELTA`** (Devolución total o parcial post-cierre, requiere NDC).

*Nota:* No incluir estados de pago (PAGADA, PARCIAL) aquí. Eso pertenece al dominio **Pagos**. Una Venta es COMPLETADA comercialmente; su deuda es gestionada en otro lado.

## 8. Flujo POS (Venta Directa)
1. POS construye Venta en memoria.
2. API recibe `CrearVentaRequest`.
3. Valida disponibilidad de Stock (Service).
4. Guarda Venta + Detalles (Snapshot de precios).
5. Invoca actualización de Stock.
6. Retorna Venta.
7. POS dispara interfaz de Pago.

## 9. Pedido → Venta
1. Cajero carga `Pedido` (Estado = CONFIRMADO).
2. Llama a `POST /api/v1/ventas/desde-pedido/{idPedido}` con `Idempotency-Key` y `If-Match` del Pedido.
3. Se clonan detalles hacia `VentaDetalles`.
4. El `Pedido` cambia a estado `ATENDIDO` (transacción atómica con la Venta).
5. Restricción SQL `UQ_Ventas_IdPedido_IdEmpresa` garantiza que no existan dos ventas para el mismo pedido.

## 10. Stock
- **Venta descuenta stock a través del `IInventarioService`**. 
- Nunca se cruzan consultas directas al contexto de Inventario desde el Controlador de Ventas; la Venta notifica al servicio de dominio correspondiente para emitir el `MovimientoInventario`.

## 11. Reserva de Stock de Pedidos
- Las reservas se gestionan en **Inventario**.
- Cuando el `Pedido` pasa a `CONFIRMADO`, `PedidoService` solicita a `IInventarioService` una reserva (StockReservado += X).
- Al convertirse en `Venta`, `VentaService` indica consumir esa reserva (StockReservado -= X, StockFisico -= X). 

## 12. Pagos
- Independiente. La `Venta` registra un monto total. 
- Módulo `Pagos` creará una `CuentaPorCobrar` asociada a la `Venta`.
- No mezclar metadatos transaccionales (Visa, Efectivo) en `dbo.Ventas`.

## 13. Caja
- Una Venta NO abre caja.
- El ingreso de `Pago` es el que registra el Movimiento de Caja.

## 14. Facturación
- Es un proceso asíncrono o síncrono posterior a la Venta.
- `ComprobanteService` lee la `Venta` y genera Factura/Boleta XML para SUNAT. 
- Si SUNAT falla, la `Venta` comercial sigue viva; se reintenta el comprobante.

## 15. Concurrencia
- **RowVersion:** Integrado en `dbo.Ventas`. Si dos cajeros anulan o devuelven la misma venta, el segundo recibe 409 Conflict.
- Mismo mecanismo empleado en Pedidos.

## 16. Idempotencia
- **Constraint:** `UQ_Ventas_IdEmpresa_IdempotencyKey`.
- **Doble clic del POS:** Evita doble venta; retorna 201 o 200 con la misma venta generada en el intento 1.

## 17. Multiempresa
- Todo flujo inyecta `IdEmpresa` desde JWT a la base de datos (Composite Keys o checks automáticos).
- Filtros automáticos a nivel Application/Domain para evitar IDOR de sucursales ajenas.

## 18. Numeración
- El `NumeroVenta` será un correlativo interno (ej. `VEN-001-000045`) para control comercial.
- La numeración fiscal (ej. `F001-0000001`) le pertenece al dominio de `Facturación`.

## 19. Devoluciones
- Una devolución afecta Inventario (ingreso), Pagos (nota de crédito/deuda) y Facturación (NDC).
- `Venta` permite cambiar de estado a `DEVUELTA` si todo el flujo secundario tiene éxito.

## 20. Seguridad
- Políticas: `VENTAS_CREAR`, `VENTAS_VER`, `VENTAS_ANULAR`.
- Roles: Solo CAJERO o ADMIN pueden realizar `VENTAS_CREAR`.

## 21. Servicios
- `IVentaService`: Orquestador principal.
- `IVentaRepository`: Persistencia pura de Venta.
- Interacciones transversales: `IInventarioStockService` (invocado sincrónicamente para asegurar atomicidad comercial).

## 22. API Futura
- `POST /api/v1/ventas`
- `POST /api/v1/ventas/desde-pedido/{id}`
- `GET /api/v1/ventas/{id}`
- `PATCH /api/v1/ventas/{id}/anular`

## 23. Transacciones (Sagas vs ACID)
- **Venta + Inventario + Pedido (cambio estado):** Transacción ACID local en SQL Server (EF Core `SaveChanges`), ya que residen en la misma DB y no toleran inconsistencia.
- **Venta + Facturación:** Diseño Event-Driven o Saga síncrona tolerante a fallos. La factura se emite "después" de que la venta hace Commit.

## 24. Riesgos
| RIESGO | PROB. | IMPACTO | MITIGACIÓN | DECISIÓN |
|---|---|---|---|---|
| Doble procesamiento Pedido | Baja | Alto | Unique Index `IdPedido` en Ventas. | Congelada |
| Doble click Venta POS | Alta | Medio | Idempotency-Key obligatorio desde App POS. | Congelada |
| Venta sin stock | Media | Alto | `IVentaService` bloquea la TX si `IInventario` tira error. | Congelada |
| SUNAT caída al vender | Alta | Bajo | Separar Venta Comercial del Comprobante Fiscal. | Congelada |

## 25. Decisiones Congeladas
- Pedido y Venta son entidades separadas.
- La reserva de stock ocurre en el dominio Inventario, solicitada por Pedidos.
- Snapshot de Precios y Costos obligatorio en `VentaDetalles`.
- La Venta NO almacena la forma de pago (eso va en módulo Pagos).

## 26. Decisiones Pendientes
- ¿El cálculo de Cuentas por Cobrar ocurrirá sincrónicamente al crear la Venta o mediante mensajería en memoria? (Se decidirá en Fase Pagos).
- Flujo exacto de Apertura/Cierre de Turnos (Se decidirá en Fase Caja).
- Configuración de series para SUNAT (Se decidirá en Fase Facturación).

## 27. Impacto en dominios futuros
Este diseño cimenta la Venta como el Eje Central del ERP. De ella nacen las Cuentas por Cobrar, el descargo real del Inventario, la entrada a Caja (cuando se paga al contado) y los libros electrónicos fiscales. Cualquier cambio en Ventas requerirá validación de integración global.
