using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;
using SistemaDeCotizacion.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SistemaDeCotizacion.Controllers
{
    public class PdfConverterController : Controller
    {
        private readonly AppDBContext _appDBContext;
        public PdfConverterController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public IActionResult GenerarPDFCotizacion(int id)
        {
            var cotizacion = _appDBContext.Cotizaciones
                .Include(c => c.cliente)
                    .ThenInclude(c => c.vehiculos)
                .Include(c => c.servicios)
                    .ThenInclude(ds => ds.servicio)
                .Include(c => c.repuestos)
                    .ThenInclude(dr => dr.repuesto)
                .FirstOrDefault(c => c.cotizacion_id == id);
            if (cotizacion == null)
            {
                return NotFound();
            }

            var vm = new CotizacionVM
            {
                CotizacionId = cotizacion.cotizacion_id,
                ClienteId = cotizacion.cliente_id,
                VehiculoId = cotizacion.cliente.vehiculos.FirstOrDefault()?.vehiculo_id,
                formaPago = cotizacion.formaPago,
                tiempoEntrega = cotizacion.tiempoEntrega,
                estado_cotizacion = cotizacion.estado_cotizacion,

                ServiciosSeleccionados = cotizacion.servicios.Select(s => new ServicioSeleccionadoVM
                {
                    ServicioId = s.servicio_id,
                    precio = s.servicio.precio,
                    nombre_servicio = s.servicio.nombre_servicio
                }).ToList(),

                RepuestosSeleccionados = cotizacion.repuestos.Select(r => new RepuestoSeleccionadoVM
                {
                    RepuestoId = r.repuesto_id,
                    Cantidad = r.cantidad_rep,
                    codigo_rep = r.repuesto.codigo_rep,
                    descripcion = r.repuesto.descripcion,
                    medida_rep = r.repuesto.medida_rep,
                    precio_und = r.repuesto.precio_und
                }).ToList(),

                Clientes = _appDBContext.Clientes.ToList(),
                Vehiculos = _appDBContext.Vehiculos
                    .Where(v => v.cliente_id == cotizacion.cliente_id)
                    .ToList(),
                Servicios = _appDBContext.Servicios.ToList(),
                Repuestos = _appDBContext.Repuestos.ToList()
            };

            var empresaDireccion = "PROLONGACION AV. RICARDO PALMA URB. EL BOSQUE 1397";
            var empresaCorreo = "todocamioneseirl@gmail.com";
            var empresaTelefono = "949280381";
            var empresaCta = "CTA CTE BCP: 570-9943050-0-22";
            var empresaCCI = "CCI BCP: 002-57000994305002205";

            int numeroBase = 2749;
            int id_sum = cotizacion.cotizacion_id;
            int resultado = id_sum + numeroBase;
            string cotizacionNumero = $"000-{resultado:D7}";

            var cotizacionFecha = cotizacion.fecha_cotizacion;

            var clienteNombre = cotizacion.cliente.nombre_cliente ?? "N/A";
            var clienteRUC = cotizacion.cliente.ruc ?? "N/A";
            var clienteCorreo = cotizacion.cliente.correo_cliente ?? "N/A";
            var clienteTelefono = cotizacion.cliente.telefono_cliente ?? "N/A";

            var vehiculo = _appDBContext.Vehiculos
                .FirstOrDefault(v => v.cliente_id == cotizacion.cliente_id);


            var placa = vehiculo?.placa ?? "N/A";
            var modelo = vehiculo?.modelo ?? "N/A";
            var marca = vehiculo?.marca ?? "N/A";
            var km = vehiculo?.kilometraje.ToString() ?? "N/A";

            var servicios = vm.ServiciosSeleccionados;
            var repuestos = vm.RepuestosSeleccionados;

            var atendidoPor = "Técnico: Juan Pérez";
            var formaPago = cotizacion.formaPago;
            var tiempoEntrega = cotizacion.tiempoEntrega + " días";

            Double subtotalServicios = cotizacion.costo_servicio_total;
            Double subtotalRepuestos = cotizacion.costo_repuesto_total;
            Double subtotal = subtotalServicios + subtotalRepuestos;
            Double total = subtotal;

            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Logo.png");
            var logoExists = System.IO.File.Exists(logoPath);

            // ----------------------------
            // Crear documento con QuestPDF
            // ----------------------------
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Times New Roman"));
                    page.PageColor(Colors.White);

                    // ---------- HEADER ----------
                    page.Header()
                        .PaddingBottom(6)
                        .Row(r =>
                        {
                            // Logo (izquierda)
                            r.ConstantColumn(300).Element(left =>
                            {
                                left.Element(c =>
                                {
                                    if (logoExists)
                                    {
                                        using var fs = System.IO.File.OpenRead(logoPath);
                                        c.Image(fs, ImageScaling.FitWidth);
                                    }
                                    else
                                    {
                                        c.PaddingVertical(20).AlignCenter().Text("LOGO").FontSize(14).Bold();
                                    }
                                });
                            });

                            r.RelativeColumn().Element(_ => { /* espacio */ });

                            // Cuadro de cotización (derecha)
                            r.ConstantColumn(230).Element(right =>
                            {
                                right.AlignRight().Border(1.5f).BorderColor("#B71C1C").Background("#FFEBEE").Padding(8).Column(col =>
                                {
                                    col.Item().AlignCenter().Text("COTIZACIÓN")
                                        .FontColor("#B71C1C")
                                        .FontSize(13)
                                        .Bold();

                                    col.Item().PaddingTop(2).AlignCenter().Text($"N° {cotizacionNumero}")
                                        .FontColor("#B71C1C")
                                        .FontSize(11)
                                        .Bold();

                                    col.Item().PaddingTop(2).AlignCenter().Text($"Fecha: {cotizacionFecha}")
                                        .FontColor(Colors.Black)
                                        .FontSize(10);
                                });
                            });
                        });

                    // ---------- CONTENT ----------
                    page.Content().Column(col =>
                    {
                        // ===== EMPRESA: tabla con celdas individuales (como en tu imagen) =====
                        col.Item().PaddingBottom(6).Element(c =>
                        {
                            c.Table(table =>
                            {
                                // 4 columnas: label-left, value-left (span wide), label-right, value-right
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(80);   // etiqueta izquierda
                                    cols.RelativeColumn(5);   // valor izquierda
                                    cols.ConstantColumn(80);  // etiqueta derecha
                                    cols.RelativeColumn(4);   // valor derecha
                                });

                                // Fila 1: DIRECCIÓN / TELEFONO
                                // Fila DIRECCIÓN (etiqueta + valor unidos, sin celdas vacías extra)
                                table.Cell().Element(cell => cell
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Background("#D9D9D9")
                                    .Padding(6)
                                    .Text("DIRECCIÓN:")
                                    .Bold()
                                );

                                // 🔹 Esta celda se une con las dos columnas restantes (3 y 4)
                                table.Cell().ColumnSpan(3).Element(cell => cell
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Padding(6)
                                    .Text(empresaDireccion)
                                );


                                // Fila 2: CORREO / CTA CTE BCP
                                table.Cell().Element(cell => cell.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(6).Text("CORREO:").Bold());
                                table.Cell().Element(cell => cell.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Text(empresaCorreo));
                                table.Cell().Element(cell => cell.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(6).Text("CTA CTE BCP:").Bold());
                                table.Cell().Element(cell => cell.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Text(empresaCta));

                                // Fila 3: empty label / CCI BCP on right (keeps table shape similar to your sample)
                                table.Cell().Element(cell => cell.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(6).Text("TELEFONO:").Bold());
                                table.Cell().Element(cell => cell.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Text(empresaTelefono));
                                table.Cell().Element(cell => cell.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(6).Text("CCI BCP:").Bold());
                                table.Cell().Element(cell => cell.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Text(empresaCCI));
                            });
                        });

                        // Texto introductorio
                        col.Item().PaddingBottom(6).Text("Mediante la presente cotizamos su requerimiento como sigue a continuación:").FontSize(10);

                        // ===== DATOS DEL CLIENTE Y VEHÍCULO: ambos en tablas con celdas bordeadas =====
                        // ===== DATOS DEL CLIENTE =====
                        col.Item().Element(container =>
                        {
                            container.Table(tbl =>
                            {
                                tbl.ColumnsDefinition(cd =>
                                {
                                    cd.RelativeColumn(1);
                                    cd.RelativeColumn(2);
                                    cd.RelativeColumn(1);
                                    cd.RelativeColumn(2);
                                });

                                // Encabezado
                                tbl.Cell().ColumnSpan(4).Element(h => h
                                    .Background("#4F4F4F")
                                    .Padding(6)
                                    .Text("DATOS DEL CLIENTE")
                                    .FontColor(Colors.White)
                                    .Bold());

                                // Fila 2: Nombre/Razón Social y RUC
                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Background("#D9D9D9")
                                    .Padding(6)
                                    .Text("NOMBRE / RAZÓN SOCIAL:")
                                    .Bold());
                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Padding(6)
                                    .Text(clienteNombre));

                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Background("#D9D9D9")
                                    .Padding(6)
                                    .Text("RUC:")
                                    .Bold());
                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Padding(6)
                                    .Text(clienteRUC));

                                // Fila 3: Correo y Teléfono
                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Background("#D9D9D9")
                                    .Padding(6)
                                    .Text("CORREO:")
                                    .Bold());
                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Padding(6)
                                    .Text(clienteCorreo));

                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Background("#D9D9D9")
                                    .Padding(6)
                                    .Text("TELÉFONO:")
                                    .Bold());
                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Padding(6)
                                    .Text(clienteTelefono));
                            });
                        });

                        col.Item().PaddingTop(10);

                        // ===== DATOS DEL VEHÍCULO =====
                        col.Item().Element(container =>
                        {
                            container.Table(tbl =>
                            {
                                tbl.ColumnsDefinition(cd =>
                                {
                                    cd.RelativeColumn(1);
                                    cd.RelativeColumn(2);
                                    cd.RelativeColumn(1);
                                    cd.RelativeColumn(2);
                                });

                                // Encabezado
                                tbl.Cell().ColumnSpan(4).Element(h => h
                                    .Background("#4F4F4F")
                                    .Padding(6)
                                    .Text("DATOS DEL VEHÍCULO")
                                    .FontColor(Colors.White)
                                    .Bold());

                                // Fila 2: Placa y Modelo
                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Background("#D9D9D9")
                                    .Padding(6)
                                    .Text("PLACA:")
                                    .Bold());
                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Padding(6)
                                    .Text(placa));

                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Background("#D9D9D9")
                                    .Padding(6)
                                    .Text("MODELO:")
                                    .Bold());
                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Padding(6)
                                    .Text(modelo));

                                // Fila 3: Marca y KM
                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Background("#D9D9D9")
                                    .Padding(6)
                                    .Text("MARCA:")
                                    .Bold());
                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Padding(6)
                                    .Text(marca));

                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Background("#D9D9D9")
                                    .Padding(6)
                                    .Text("KM:")
                                    .Bold());
                                tbl.Cell().Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Padding(6)
                                    .Text(km));
                            });
                        });

                        col.Item().PaddingTop(8);

                        // ===== SERVICIOS - tabla con celdas bordeadas =====
                        col.Item().Element(tcont =>
                        {
                            tcont.Table(tbl =>
                            {
                                tbl.ColumnsDefinition(cd => { cd.ConstantColumn(30); cd.RelativeColumn(7); cd.ConstantColumn(80); });

                                // Header row (span full)
                                tbl.Cell().ColumnSpan(3).Element(h => h.Background("#4F4F4F").Padding(6).Text("SERVICIOS A REALIZAR").FontColor(Colors.White).Bold());

                                // Column titles
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(6).Text("N°").Bold().AlignCenter());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(6).Text("DESCRIPCIÓN").Bold().AlignCenter());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(6).Text("COSTO DEL SERVICIO S/").Bold().AlignCenter());

                                int i = 1;
                                foreach (var s in servicios)
                                {
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).AlignCenter().Text(i.ToString()));
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Text(s.nombre_servicio));
                                    var precio = s.precio > 0 ? $"S/ {s.precio:N2}" : "";
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).AlignRight().Text(precio));
                                    i++;
                                }

                                // total servicios row
                                tbl.Cell().ColumnSpan(2).Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Background("#F3F3F3").AlignRight().Text("TOTAL SERVICIOS:").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Background("#F3F3F3").AlignRight().Text($"S/ {subtotalServicios:N2}").Bold());
                            });
                        });

                        col.Item().PaddingTop(6);

                        // ===== REPUESTOS - tabla con celdas bordeadas =====
                        col.Item().Element(tcont =>
                        {
                            tcont.Table(tbl =>
                            {
                                tbl.ColumnsDefinition(cd => { cd.RelativeColumn(1); cd.ConstantColumn(50); cd.ConstantColumn(50); cd.RelativeColumn(4); cd.ConstantColumn(70); cd.ConstantColumn(70); });

                                tbl.Cell().ColumnSpan(6).Element(h => h.Background("#4F4F4F").Padding(6).Text("REPUESTOS Y MATERIALES").FontColor(Colors.White).Bold());

                                // headers
                                var headers = new[] { "CÓDIGO", "CANT.", "MEDIDA", "DESCRIPCIÓN", "P. UNIT.", "VALOR VENTA" };
                                foreach (var head in headers)
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(6).Text(head).Bold().AlignCenter());

                                foreach (var ritem in repuestos)
                                {
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).AlignCenter().Text(ritem.codigo_rep));
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).AlignCenter().Text(ritem.Cantidad.ToString()));
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).AlignCenter().Text(ritem.medida_rep));
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Text(ritem.descripcion));
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).AlignRight().Text($"S/ {ritem.precio_und:N2}"));
                                    var valor = subtotalRepuestos;
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).AlignRight().Text($"S/ {valor:N2}"));
                                }

                                // total repuestos
                                tbl.Cell().ColumnSpan(5).Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Background("#F3F3F3").AlignRight().Text("TOTAL REPUESTOS:").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Background("#F3F3F3").AlignRight().Text($"S/ {subtotalRepuestos:N2}").Bold());
                            });
                        });

                        col.Item().PaddingTop(8);

                        // ===== TOTALES (solo importe total) =====
                        col.Item().Element(totalBox =>
                        {
                            totalBox.Table(tbl =>
                            {
                                tbl.ColumnsDefinition(cd => { cd.RelativeColumn(6); cd.ConstantColumn(140); });

                                // Left big cell for "SON: ..." (two rows)
                                tbl.Cell().Row(1).Column(1).RowSpan(2).Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Text("_____________________________________________").FontSize(9));

                                // Label Importe total
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(6).AlignRight().Text("IMPORTE TOTAL").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).AlignRight().Text($"S/ {total:N2}").Bold());
                            });
                        });

                        col.Item().PaddingTop(8);

                        // ===== ATENCIÓN / CONDICIONES =====
                        col.Item().Element(at =>
                        {
                            at.Table(tbl =>
                            {
                                tbl.ColumnsDefinition(cd =>
                                {
                                    cd.ConstantColumn(120); // primera columna
                                    cd.RelativeColumn(4);   // segunda columna
                                });

                                // 🔹 Subtítulo: una sola celda que abarca ambas columnas
                                tbl.Cell().ColumnSpan(2).Element(c => c
                                    .Border(0.8f)
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Background("#4F4F4F")
                                    .Padding(6)
                                    .Text("ATENCIÓN / CONDICIONES")
                                    .FontColor(Colors.White)
                                    .Bold()
                                    .AlignCenter()
                                );

                                // Fila 1
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Text("Atendido por:").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Text(atendidoPor));

                                // Fila 2
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Text("Forma de pago:").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Text(formaPago));

                                // Fila 3
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Text("Tiempo de entrega:").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(6).Text(tiempoEntrega));
                            });
                        });


                        col.Item().AlignCenter().Text("\n\nOPERACIÓN SUJETA AL SISTEMA DE PAGO DE OBLIGACIONES TRIBUTARIAS CON EL GOBIERNO CENTRAL Bancode la Nacion CTA DE TRACCIONES:00741755262\r\nSERVICIO MANO DE OBRA NO INCLUYE IGV").FontSize(9);

                    });

                    // FOOTER
                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("Página ");
                        txt.CurrentPageNumber();
                        txt.Span(" de ");
                        txt.TotalPages();
                    });
                });
            });

            // ----------------------------
            // Generar PDF y devolver inline (visor)
            // ----------------------------
            var pdfBytes = document.GeneratePdf();

            Response.Headers["Content-Disposition"] = "inline; filename=cotizacion.pdf";
            return new FileContentResult(pdfBytes, "application/pdf");
        }
    }
}
