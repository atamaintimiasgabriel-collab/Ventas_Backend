using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;
using SistemaVentasMotorrepuestos.API.Modelos.Facturacion.UBL;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion
{
    /// <summary>
    /// Mapper que transforma DocumentoElectronicoDTO → modelo UBL.
    /// Es una capa intermedia pura: no accede a BD, no recalcula impuestos,
    /// no llama a ningún proveedor externo.
    ///
    /// Tipos soportados:
    ///   01 - Factura
    ///   03 - Boleta de Venta
    ///   07 - Nota de Crédito
    ///   08 - Nota de Débito
    ///
    /// El método de cada tipo devuelve un object para que el serializador
    /// pueda usar XmlSerializer con el tipo concreto sin conocer cada rama.
    /// </summary>
    internal static class UblDocumentMapper
    {
        // Códigos de tipo de comprobante
        private const string Factura     = "01";
        private const string Boleta      = "03";
        private const string NotaCredito = "07";
        private const string NotaDebito  = "08";

        // SchemeIDs para tipo de documento de identidad
        // "6" = RUC, "1" = DNI, "4" = Carnet Extranjería, "7" = Pasaporte
        private const string SchemeRuc = "6";
        private const string SchemeDni = "1";

        /// <summary>
        /// Determina qué tipo de documento UBL producir y ejecuta el mapper correspondiente.
        /// </summary>
        public static object MapearDesdeDto(DocumentoElectronicoDTO dto)
        {
            ValidarDtoBase(dto);

            return dto.TipoComprobante switch
            {
                Factura     => MapearInvoice(dto, Factura),
                Boleta      => MapearInvoice(dto, Boleta),
                NotaCredito => MapearCreditNote(dto),
                NotaDebito  => MapearDebitNote(dto),
                _ => throw new XmlSerializacionException(
                    XmlSerializacionErrorTipo.TipoNoSoportado,
                    $"El tipo de comprobante '{dto.TipoComprobante}' no tiene un mapper UBL implementado. " +
                    $"Tipos soportados: {Factura} (Factura), {Boleta} (Boleta), " +
                    $"{NotaCredito} (Nota Crédito), {NotaDebito} (Nota Débito).")
            };
        }

        // ─── Invoice (Factura + Boleta) ──────────────────────────────────────────

        private static UblInvoice MapearInvoice(DocumentoElectronicoDTO dto, string invoiceTypeCode)
        {
            var invoice = new UblInvoice
            {
                ID                     = BuildDocumentId(dto),
                IssueDate              = dto.FechaEmision.ToString("yyyy-MM-dd"),
                IssueTime              = dto.FechaEmision.ToString("HH:mm:ss"),
                ProfileID              = dto.TipoOperacionSunat,
                InvoiceTypeCode        = invoiceTypeCode,
                DocumentCurrencyCode   = dto.Moneda,
                AccountingSupplierParty = BuildEmisorParty(dto),
                AccountingCustomerParty = BuildClienteParty(dto),
                TaxTotals              = BuildTaxTotals(dto),
                LegalMonetaryTotal     = BuildLegalMonetaryTotal(dto),
                InvoiceLines           = BuildInvoiceLines(dto),
            };

            return invoice;
        }

        // ─── Credit Note ─────────────────────────────────────────────────────────

        private static UblCreditNote MapearCreditNote(DocumentoElectronicoDTO dto)
        {
            return new UblCreditNote
            {
                ID                      = BuildDocumentId(dto),
                IssueDate               = dto.FechaEmision.ToString("yyyy-MM-dd"),
                IssueTime               = dto.FechaEmision.ToString("HH:mm:ss"),
                DocumentCurrencyCode    = dto.Moneda,
                DiscrepancyResponse     = BuildDiscrepancyResponse(dto),
                BillingReference        = BuildBillingReference(dto),
                AccountingSupplierParty  = BuildEmisorParty(dto),
                AccountingCustomerParty  = BuildClienteParty(dto),
                TaxTotals               = BuildTaxTotals(dto),
                LegalMonetaryTotal      = BuildLegalMonetaryTotal(dto),
                CreditNoteLines         = BuildCreditNoteLines(dto),
            };
        }

        // ─── Debit Note ──────────────────────────────────────────────────────────

        private static UblDebitNote MapearDebitNote(DocumentoElectronicoDTO dto)
        {
            return new UblDebitNote
            {
                ID                      = BuildDocumentId(dto),
                IssueDate               = dto.FechaEmision.ToString("yyyy-MM-dd"),
                IssueTime               = dto.FechaEmision.ToString("HH:mm:ss"),
                DocumentCurrencyCode    = dto.Moneda,
                DiscrepancyResponse     = BuildDiscrepancyResponse(dto),
                BillingReference        = BuildBillingReference(dto),
                AccountingSupplierParty  = BuildEmisorParty(dto),
                AccountingCustomerParty  = BuildClienteParty(dto),
                TaxTotals               = BuildTaxTotals(dto),
                RequestedMonetaryTotal  = BuildLegalMonetaryTotal(dto),
                DebitNoteLines          = BuildDebitNoteLines(dto),
            };
        }

        // ─── Partes ──────────────────────────────────────────────────────────────

        private static UblAccountingParty BuildEmisorParty(DocumentoElectronicoDTO dto)
        {
            return new UblAccountingParty
            {
                Party = new UblParty
                {
                    PartyIdentification = new UblPartyIdentification
                    {
                        ID = new UblIdWithScheme
                        {
                            SchemeID = SchemeRuc,
                            Value    = dto.Emisor.Ruc
                        }
                    },
                    PartyLegalEntity = new UblPartyLegalEntity
                    {
                        RegistrationName = dto.Emisor.RazonSocial,
                        RegistrationAddress = dto.Emisor.Direccion is not null
                            ? new UblRegistrationAddress
                              {
                                  AddressLine = new UblAddressLine { Line = dto.Emisor.Direccion }
                              }
                            : null
                    }
                }
            };
        }

        private static UblAccountingParty BuildClienteParty(DocumentoElectronicoDTO dto)
        {
            if (dto.Cliente is null)
                throw new XmlSerializacionException(
                    XmlSerializacionErrorTipo.DatoObligatiorioFaltante,
                    "El documento no tiene datos de cliente (Cliente es null).");

            var schemeId = ResolverSchemeIdCliente(dto.Cliente.TipoDocumentoIdentidad);

            return new UblAccountingParty
            {
                Party = new UblParty
                {
                    PartyIdentification = new UblPartyIdentification
                    {
                        ID = new UblIdWithScheme
                        {
                            SchemeID = schemeId,
                            Value    = dto.Cliente.NumeroDocumento
                        }
                    },
                    PartyName = new UblPartyName
                    {
                        Name = dto.Cliente.RazonSocial
                    },
                    PartyLegalEntity = new UblPartyLegalEntity
                    {
                        RegistrationName = dto.Cliente.RazonSocial,
                        RegistrationAddress = dto.Cliente.Direccion is not null
                            ? new UblRegistrationAddress
                              {
                                  AddressLine = new UblAddressLine { Line = dto.Cliente.Direccion }
                              }
                            : null
                    },
                    Contact = dto.Cliente.Email is not null
                        ? new UblContact { ElectronicMail = dto.Cliente.Email }
                        : null
                }
            };
        }

        // ─── Impuestos ───────────────────────────────────────────────────────────

        private static List<UblTaxTotal> BuildTaxTotals(DocumentoElectronicoDTO dto)
        {
            var subtotalesPorTasa = dto.Detalles
                .GroupBy(d => new { d.PorcentajeImpuesto, d.CodigoAfectacionIgv, d.CodigoTributo, d.NombreTributo, d.CodigoInternacionalTributo })
                .Select(g => new UblTaxSubtotal
                {
                    TaxableAmount = new UblAmount { CurrencyID = dto.Moneda, Value = g.Sum(d => d.ValorVenta) },
                    TaxAmount     = new UblAmount { CurrencyID = dto.Moneda, Value = g.Sum(d => d.MontoImpuesto) },
                    TaxCategory   = new UblTaxCategory
                    {
                        ID             = ResolverCategoriaImpuesto(g.Key.CodigoAfectacionIgv),
                        Percent        = g.Key.PorcentajeImpuesto.ToString("F2"),
                        TaxExemptionReasonCode = g.Key.CodigoAfectacionIgv,
                        TaxScheme = new UblTaxScheme
                        {
                            ID           = g.Key.CodigoTributo,
                            Name         = g.Key.NombreTributo,
                            TaxTypeCode  = g.Key.CodigoInternacionalTributo
                        }
                    }
                })
                .ToList();

            return new List<UblTaxTotal>
            {
                new UblTaxTotal
                {
                    TaxAmount    = new UblAmount { CurrencyID = dto.Moneda, Value = dto.TotalIgv },
                    TaxSubtotals = subtotalesPorTasa
                }
            };
        }

        // ─── Totales ─────────────────────────────────────────────────────────────

        private static UblLegalMonetaryTotal BuildLegalMonetaryTotal(DocumentoElectronicoDTO dto)
        {
            // Valores obtenidos directamente del snapshot del DTO — sin recalcular
            var totalBase = dto.TotalGravado;

            return new UblLegalMonetaryTotal
            {
                LineExtensionAmount   = new UblAmount { CurrencyID = dto.Moneda, Value = totalBase },
                TaxExclusiveAmount    = new UblAmount { CurrencyID = dto.Moneda, Value = totalBase },
                TaxInclusiveAmount    = new UblAmount { CurrencyID = dto.Moneda, Value = dto.ImporteTotal },
                AllowanceTotalAmount  = dto.TotalDescuento > 0
                    ? new UblAmount { CurrencyID = dto.Moneda, Value = dto.TotalDescuento }
                    : null,
                PayableAmount         = new UblAmount { CurrencyID = dto.Moneda, Value = dto.ImporteTotal }
            };
        }

        // ─── Líneas ──────────────────────────────────────────────────────────────

        /// <summary>Construye la lista de líneas de Factura/Boleta (InvoicedQuantity).</summary>
        private static List<UblInvoiceLine> BuildInvoiceLines(DocumentoElectronicoDTO dto)
        {
            return dto.Detalles.Select(d => new UblInvoiceLine
            {
                ID                  = d.Secuencia.ToString(),
                Quantity            = new UblQuantity { UnitCode = d.UnidadMedidaSunat, Value = d.Cantidad },
                LineExtensionAmount = new UblAmount { CurrencyID = dto.Moneda, Value = d.ValorVenta },
                PricingReference    = BuildPricingReference(dto.Moneda, d.PrecioUnitario, d.TipoPrecio),
                AllowanceCharge     = BuildAllowanceCharge(dto.Moneda, d.Descuento),
                TaxTotal            = BuildLineTaxTotal(dto.Moneda, d),
                Item                = BuildItem(d),
                Price               = BuildPrice(dto.Moneda, d),
            }).ToList();
        }

        /// <summary>Construye la lista de líneas de Nota de Crédito (CreditedQuantity).</summary>
        private static List<UblCreditNoteLine> BuildCreditNoteLines(DocumentoElectronicoDTO dto)
        {
            return dto.Detalles.Select(d => new UblCreditNoteLine
            {
                ID                  = d.Secuencia.ToString(),
                Quantity            = new UblQuantity { UnitCode = d.UnidadMedidaSunat, Value = d.Cantidad },
                LineExtensionAmount = new UblAmount { CurrencyID = dto.Moneda, Value = d.ValorVenta },
                PricingReference    = BuildPricingReference(dto.Moneda, d.PrecioUnitario, d.TipoPrecio),
                AllowanceCharge     = BuildAllowanceCharge(dto.Moneda, d.Descuento),
                TaxTotal            = BuildLineTaxTotal(dto.Moneda, d),
                Item                = BuildItem(d),
                Price               = BuildPrice(dto.Moneda, d),
            }).ToList();
        }

        /// <summary>Construye la lista de líneas de Nota de Débito (DebitedQuantity).</summary>
        private static List<UblDebitNoteLine> BuildDebitNoteLines(DocumentoElectronicoDTO dto)
        {
            return dto.Detalles.Select(d => new UblDebitNoteLine
            {
                ID                  = d.Secuencia.ToString(),
                Quantity            = new UblQuantity { UnitCode = d.UnidadMedidaSunat, Value = d.Cantidad },
                LineExtensionAmount = new UblAmount { CurrencyID = dto.Moneda, Value = d.ValorVenta },
                PricingReference    = BuildPricingReference(dto.Moneda, d.PrecioUnitario, d.TipoPrecio),
                AllowanceCharge     = BuildAllowanceCharge(dto.Moneda, d.Descuento),
                TaxTotal            = BuildLineTaxTotal(dto.Moneda, d),
                Item                = BuildItem(d),
                Price               = BuildPrice(dto.Moneda, d),
            }).ToList();
        }

        // ─── Helpers de línea ────────────────────────────────────────────────────

        private static UblPricingReference BuildPricingReference(string moneda, decimal precioUnitario, string tipoPrecio)
            => new UblPricingReference
            {
                AlternativeConditionPrice = new UblAlternativeConditionPrice
                {
                    PriceAmount  = new UblAmount { CurrencyID = moneda, Value = precioUnitario },
                    PriceTypeCode = tipoPrecio
                }
            };

        private static UblAllowanceCharge? BuildAllowanceCharge(string moneda, decimal descuento)
            => descuento > 0
                ? new UblAllowanceCharge
                  {
                      ChargeIndicator = false,
                      Amount = new UblAmount { CurrencyID = moneda, Value = descuento }
                  }
                : null;

        private static UblTaxTotal BuildLineTaxTotal(string moneda, DetalleDocumentoDTO d)
            => new UblTaxTotal
            {
                TaxAmount    = new UblAmount { CurrencyID = moneda, Value = d.MontoImpuesto },
                TaxSubtotals = new List<UblTaxSubtotal>
                {
                    new UblTaxSubtotal
                    {
                        TaxableAmount = new UblAmount { CurrencyID = moneda, Value = d.ValorVenta },
                        TaxAmount     = new UblAmount { CurrencyID = moneda, Value = d.MontoImpuesto },
                        TaxCategory   = new UblTaxCategory
                        {
                            ID      = ResolverCategoriaImpuesto(d.CodigoAfectacionIgv),
                            Percent = d.PorcentajeImpuesto.ToString("F2"),
                            TaxExemptionReasonCode = d.CodigoAfectacionIgv,
                            TaxScheme = new UblTaxScheme
                            {
                                ID = d.CodigoTributo,
                                Name = d.NombreTributo,
                                TaxTypeCode = d.CodigoInternacionalTributo
                            }
                        }
                    }
                }
            };

        private static UblItem BuildItem(DetalleDocumentoDTO d)
            => new UblItem
            {
                Description = d.Descripcion,
                SellersItemIdentification = !string.IsNullOrWhiteSpace(d.CodigoProducto)
                    ? new UblSellersItemIdentification { ID = d.CodigoProducto }
                    : null
            };

        private static UblPrice BuildPrice(string moneda, DetalleDocumentoDTO d)
            => new UblPrice
            {
                PriceAmount = new UblAmount
                {
                    CurrencyID = moneda,
                    Value = d.Cantidad != 0 ? Math.Round(d.ValorVenta / d.Cantidad, 10) : 0m
                }
            };

        // ─── Helpers ─────────────────────────────────────────────────────────────

        private static string BuildDocumentId(DocumentoElectronicoDTO dto) =>
            $"{dto.Serie}-{dto.Correlativo:D8}";

        private static string ResolverSchemeIdCliente(string tipoDocumento)
        {
            return tipoDocumento?.ToUpperInvariant() switch
            {
                "RUC" => "6",
                "DNI" => "1",
                "CE"  => "4",
                "PASAPORTE" or "PAS" => "7",
                "SIN DOCUMENTO" or "SIN_DOCUMENTO" => "0",
                _ => throw new XmlSerializacionException(XmlSerializacionErrorTipo.DtoInvalido,
                    "El tipo de identidad del cliente no tiene equivalencia SUNAT registrada.")
            };
        }

        private static string ResolverCategoriaImpuesto(string codigoAfectacion)
        {
            return codigoAfectacion switch
            {
                "10" or "17" => "S",
                "20" => "E",
                "30" or "40" => "O",
                "11" or "12" or "13" or "14" or "15" or "16" or "21" or "31" or "32" or "33" or "34" or "35" or "36" or "37" => "Z",
                _ => throw new XmlSerializacionException(XmlSerializacionErrorTipo.DtoInvalido,
                    "El código de afectación no tiene categoría UBL registrada.")
            };
        }

        private static UblDiscrepancyResponse BuildDiscrepancyResponse(DocumentoElectronicoDTO dto)
        {
            var r = dto.DocumentoRelacionado!;
            return new UblDiscrepancyResponse
            {
                ReferenceID = $"{r.Serie}-{r.Correlativo:D8}",
                ResponseCode = r.CodigoMotivo,
                Description = r.Sustento
            };
        }

        private static UblBillingReference BuildBillingReference(DocumentoElectronicoDTO dto)
        {
            var r = dto.DocumentoRelacionado!;
            return new UblBillingReference
            {
                InvoiceDocumentReference = new UblDocumentReference
                {
                    ID = $"{r.Serie}-{r.Correlativo:D8}",
                    DocumentTypeCode = r.TipoDocumento
                }
            };
        }

        // ─── Validación básica del DTO ───────────────────────────────────────────

        private static void ValidarDtoBase(DocumentoElectronicoDTO dto)
        {
            if (dto is null)
                throw new XmlSerializacionException(
                    XmlSerializacionErrorTipo.DtoInvalido,
                    "El DocumentoElectronicoDTO es null.");

            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.TipoComprobante))
                errores.Add("TipoComprobante es requerido.");
            else if (dto.TipoComprobante is not (Factura or Boleta or NotaCredito or NotaDebito))
                errores.Add("TipoComprobante debe usar un código fiscal canónico soportado.");
            if (string.IsNullOrWhiteSpace(dto.Serie))
                errores.Add("Serie es requerida.");
            if (dto.Correlativo <= 0)
                errores.Add("Correlativo debe ser mayor a cero.");
            if (string.IsNullOrWhiteSpace(dto.Moneda))
                errores.Add("Moneda es requerida.");
            if (dto.Cliente is null)
                errores.Add("Cliente es requerido.");
            if (dto.Emisor is null)
                errores.Add("Emisor es requerido.");
            else
            {
                if (dto.Emisor.IdEmpresa != dto.IdEmpresa)
                    errores.Add("El emisor no pertenece a la empresa del documento.");
                if (string.IsNullOrWhiteSpace(dto.Emisor.Ruc) ||
                    dto.Emisor.Ruc.Length != 11 ||
                    !dto.Emisor.Ruc.All(char.IsDigit))
                    errores.Add("El RUC del emisor debe contener 11 dígitos.");
                if (string.IsNullOrWhiteSpace(dto.Emisor.RazonSocial))
                    errores.Add("La razón social del emisor es requerida.");
            }
            if (dto.Detalles is null || dto.Detalles.Count == 0)
                errores.Add("El documento debe tener al menos una línea de detalle.");
            if (dto.TipoComprobante is NotaCredito or NotaDebito && dto.DocumentoRelacionado is null)
                errores.Add("Las notas requieren el documento afectado, motivo y sustento.");

            if (errores.Count > 0)
                throw new XmlSerializacionException(
                    XmlSerializacionErrorTipo.DatoObligatiorioFaltante,
                    $"El DocumentoElectronicoDTO tiene {errores.Count} campo(s) requerido(s) faltantes.",
                    errores);
        }
    }
}
