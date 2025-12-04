using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;
using SistemaDeCotizacion.ViewModels;
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

        public static string ConvertirNumeroEnLetras(double numero)
        {
            long parteEntera = (long)Math.Truncate(numero);
            int parteDecimal = (int)Math.Round((numero - parteEntera) * 100);

            string textoEntero = NumeroALetras(parteEntera).ToUpper();
            string textoDecimal = parteDecimal.ToString("00");

            return $"{textoEntero} CON {textoDecimal}/100";
        }

        private static string NumeroALetras(long value)
        {
            if (value == 0) return "cero";
            if (value < 20) return ConvertirUnidad(value);
            if (value < 100) return ConvertirDecena(value);
            if (value < 1000) return ConvertirCentena(value);
            if (value < 1_000_000) return ConvertirMiles(value);
            if (value < 1_000_000_000_000) return ConvertirMillones(value);

            return "";
        }

        private static string ConvertirUnidad(long value)
        {
            string[] unidades = {
        "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve", "diez",
        "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve"
    };

            return unidades[value];
        }

        private static string ConvertirDecena(long value)
        {
            string[] decenas = {
        "", "", "veinte", "treinta", "cuarenta", "cincuenta",
        "sesenta", "setenta", "ochenta", "noventa"
    };

            long unidad = value % 10;
            return decenas[value / 10] + (unidad > 0 ? " y " + ConvertirUnidad(unidad) : "");
        }

        private static string ConvertirCentena(long value)
        {
            string[] centenas = {
        "", "cien", "doscientos", "trescientos", "cuatrocientos",
        "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos"
    };

            if (value == 100) return "cien";

            long resto = value % 100;
            return centenas[value / 100] + (resto > 0 ? " " + NumeroALetras(resto) : "");
        }

        private static string ConvertirMiles(long value)
        {
            long miles = value / 1000;
            long resto = value % 1000;

            string milesTexto = miles == 1 ? "mil" : NumeroALetras(miles) + " mil";

            return milesTexto + (resto > 0 ? " " + NumeroALetras(resto) : "");
        }

        private static string ConvertirMillones(long value)
        {
            long millones = value / 1_000_000;
            long resto = value % 1_000_000;

            string millonesTexto = millones == 1 ? "un millón" : NumeroALetras(millones) + " millones";

            return millonesTexto + (resto > 0 ? " " + NumeroALetras(resto) : "");
        }


        [HttpGet]
        public IActionResult GenerarPDFCotizacion(int id)
        {
            if (!ModelState.IsValid)
            {
                return View(id);
            }

            var cotizacion = _appDBContext.Cotizaciones
                .Include(c => c.cliente)
                    .ThenInclude(c => c.vehiculos)
                .Include(c => c.servicios)
                    .ThenInclude(ds => ds.servicio)
                .Include(c => c.repuestos)
                    .ThenInclude(dr => dr.repuesto)
                .FirstOrDefault(c => c.cotizacion_id == id);

            if (cotizacion == null)
                return NotFound();

            var vm = new CotizacionVM
            {
                CotizacionId = cotizacion.cotizacion_id,
                ClienteId = cotizacion.cliente_id,
                VehiculoId = cotizacion.cliente.vehiculos.FirstOrDefault()?.vehiculo_id,
                formaPago = cotizacion.formaPago,
                tiempoEntrega = cotizacion.tiempoEntrega,
                trabajador = cotizacion.trabajador,
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
                Vehiculos = _appDBContext.Vehiculos.Where(v => v.cliente_id == cotizacion.cliente_id).ToList(),
                Servicios = _appDBContext.Servicios.ToList(),
                Repuestos = _appDBContext.Repuestos.ToList()
            };

            var empresaDireccion = "PROLONGACION AV. RICARDO PALMA URB. EL BOSQUE 1397";
            var empresaCorreo = "todocamioneseirl@gmail.com";
            var empresaTelefono = "949280381";
            var empresaCta = "570-9943050-0-22";
            var empresaCCI = "002-57000994305002205";

            int numeroBase = 2749;
            int resultado = cotizacion.cotizacion_id + numeroBase;
            string cotizacionNumero = $"000-{resultado:D7}";

            var cotizacionFecha = cotizacion.fecha_cotizacion;

            var clienteNombre = cotizacion.cliente.nombre_cliente ?? "N/A";
            var clienteRUC = cotizacion.cliente.ruc ?? "N/A";
            var clienteCorreo = cotizacion.cliente.correo_cliente ?? "N/A";
            var clienteTelefono = cotizacion.cliente.telefono_cliente ?? "N/A";

            var vehiculo = _appDBContext.Vehiculos.FirstOrDefault(v => v.cliente_id == cotizacion.cliente_id);
            var placa = vehiculo?.placa ?? "N/A";
            var modelo = vehiculo?.modelo ?? "N/A";
            var marca = vehiculo?.marca ?? "N/A";
            var km = vehiculo?.kilometraje.ToString() ?? "N/A";

            var servicios = vm.ServiciosSeleccionados;
            var repuestos = vm.RepuestosSeleccionados;

            var atendidoPor = vm.trabajador;
            var formaPago = cotizacion.formaPago;
            var tiempoEntrega = cotizacion.tiempoEntrega + " días";

            double subtotalServicios = cotizacion.costo_servicio_total;
            double subtotalRepuestos = cotizacion.costo_repuesto_total;
            double total = subtotalServicios + subtotalRepuestos;
            string totalEnLetras = ConvertirNumeroEnLetras(total);

            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Logo.png");
            var logoExists = System.IO.File.Exists(logoPath);
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30); // más compacto
                    page.DefaultTextStyle(x => x.FontSize(8.5f).FontFamily("Times New Roman"));
                    page.PageColor(Colors.White);

                    // HEADER
                    page.Header().PaddingBottom(3).Row(r =>
                    {
                        r.ConstantColumn(260).Element(left =>
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
                                    c.PaddingVertical(10).AlignCenter().Text("LOGO").FontSize(20).Bold();
                                }
                            });
                        });

                        r.RelativeColumn();

                        r.ConstantColumn(200).Element(right =>
                        {
                            right.AlignRight()
                                .Border(1)
                                .BorderColor("#000000")
                                .Background("#FFFFFF")
                                .Padding(4)
                                .Column(col =>
                                {
                                    col.Item().AlignCenter().Text("20610683151").FontColor("#000000").FontSize(15).Bold();
                                    col.Item().AlignCenter().Text("COTIZACIÓN").FontColor("#000000").FontSize(16).Bold();
                                    col.Item().AlignCenter().Text($"N° {cotizacionNumero}").FontColor("#000000").FontSize(16).Bold();
                                    col.Item().AlignCenter()
                                       .Text($"Fecha: {cotizacionFecha:dd/MM/yyyy hh:mm tt}")
                                       .FontColor(Colors.Black)
                                       .FontSize(8);


                                });
                        });
                    });

                    // CONTENT
                    page.Content().Column(col =>
                    {
                        // EMPRESA
                        col.Item().PaddingBottom(3).Element(c =>
                        {
                            c.Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(70);
                                    cols.RelativeColumn(5);
                                    cols.ConstantColumn(70);
                                    cols.RelativeColumn(4);
                                });

                                table.Cell().Element(cell => cell.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("DIRECCIÓN:").Bold());
                                table.Cell().ColumnSpan(3).Element(cell => cell.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Text(empresaDireccion).Bold());

                                table.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("CORREO:").Bold());
                                table.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Text(empresaCorreo).FontColor("#007BFF"));

                                table.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("CTA CTE BCP:").Bold());
                                table.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Text(empresaCta));

                                table.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("TELÉFONO:").Bold());
                                table.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Text(empresaTelefono));
                                table.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("CCI BCP:").Bold());
                                table.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Text(empresaCCI));
                            });
                        });

                        col.Item().PaddingBottom(3).Text("Mediante la presente cotizamos su requerimiento como sigue a continuación:").FontSize(8.5f);

                        // DATOS DEL CLIENTE
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

                                tbl.Cell().ColumnSpan(4).Element(h => h.BorderColor(Colors.Black)
                                            .Background(Colors.Black).Padding(3).Text("DATOS DEL CLIENTE").FontColor(Colors.White).Bold().AlignCenter());

                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("NOMBRE / RAZÓN SOCIAL:").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Text(clienteNombre));

                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("RUC:").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Text(clienteRUC));

                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("CORREO:").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Text(clienteCorreo));

                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("TELÉFONO:").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Text(clienteTelefono));
                            });
                        });

                        col.Item().PaddingTop(3);

                        // DATOS DEL VEHÍCULO
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

                                tbl.Cell().ColumnSpan(4).Element(h => h.BorderColor(Colors.Black)
                                            .Background(Colors.Black).Padding(3).Text("DATOS DEL VEHÍCULO").FontColor(Colors.White).Bold().AlignCenter());

                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("PLACA:").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Text(placa));

                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("MODELO:").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Text(modelo));

                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("MARCA:").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Text(marca));

                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("KM:").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Text(km));
                            });
                        });

                        col.Item().PaddingTop(3);

                        // SERVICIOS
                        col.Item().Element(tcont =>
                        {
                            tcont.Table(tbl =>
                            {
                                tbl.ColumnsDefinition(cd => { cd.ConstantColumn(25); cd.RelativeColumn(7); cd.ConstantColumn(70); });

                                tbl.Cell().ColumnSpan(3).Element(h => h.BorderColor(Colors.Black)
                                            .Background(Colors.Black).Padding(3).Text("SERVICIOS A REALIZAR").FontColor(Colors.White).Bold().AlignCenter());

                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("N°").Bold().AlignCenter());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("DESCRIPCIÓN").Bold().AlignCenter());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text("COSTO").Bold().AlignCenter());

                                int i = 1;
                                foreach (var s in servicios)
                                {
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).AlignCenter().Text(i.ToString()));
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Text(s.nombre_servicio));
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).AlignRight().Text($"S/ {s.precio:N2}"));
                                    i++;
                                }

                                tbl.Cell().ColumnSpan(2).Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Background("#F3F3F3").AlignRight().Text("TOTAL SERVICIOS:").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Background("#F3F3F3").AlignRight().Text($"S/ {subtotalServicios:N2}").Bold());
                            });
                        });

                        col.Item().PaddingTop(3);

                        // REPUESTOS
                        col.Item().Element(tcont =>
                        {
                            tcont.Table(tbl =>
                            {
                                tbl.ColumnsDefinition(cd =>
                                {
                                    cd.RelativeColumn(1);
                                    cd.ConstantColumn(40);
                                    cd.ConstantColumn(40);
                                    cd.RelativeColumn(4);
                                    cd.ConstantColumn(60);
                                    cd.ConstantColumn(60);
                                });

                                tbl.Cell().ColumnSpan(6).Element(h => h.BorderColor(Colors.Black)
                                            .Background(Colors.Black).Padding(3).Text("REPUESTOS Y MATERIALES").FontColor(Colors.White).Bold().AlignCenter());

                                var headers = new[] { "CÓDIGO", "CANT.", "MEDIDA", "DESCRIPCIÓN", "P. UNIT.", "VALOR" };
                                foreach (var head in headers)
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Background("#D9D9D9").Padding(3).Text(head).Bold().AlignCenter());

                                foreach (var ritem in repuestos)
                                {
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).AlignCenter().Text(ritem.codigo_rep));
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).AlignCenter().Text(ritem.Cantidad.ToString()));
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).AlignCenter().Text(ritem.medida_rep));
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Text(ritem.descripcion));
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).AlignRight().Text($"S/ {ritem.precio_und:N2}"));
                                    tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).AlignRight().Text($"S/ {(ritem.Cantidad * ritem.precio_und):N2}"));
                                }

                                tbl.Cell().ColumnSpan(5).Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Background("#F3F3F3").AlignRight().Text("TOTAL REPUESTOS:").Bold());
                                tbl.Cell().Element(c => c.Border(0.8f).BorderColor(Colors.Grey.Darken2).Padding(3).Background("#F3F3F3").AlignRight().Text($"S/ {subtotalRepuestos:N2}").Bold());
                            });
                        });

                        col.Item().PaddingTop(3);
                        // Salto antes
                        col.Item().PaddingTop(6);

                        // Texto simple, fuera de tabla (NO usa Element)
                        col.Item().Text($"SON: {totalEnLetras} N.S")
                            .Bold()
                            .FontSize(8.5f);

                        // Salto después
                        col.Item().PaddingBottom(6);


                        col.Item().PaddingTop(3);


                        col.Item().PaddingTop(2).Element(section =>
                        {
                            section.Row(row =>
                            {

                                row.ConstantColumn(280).Element(at =>
                                {
                                    at.Table(tbl =>
                                    {
                                        tbl.ColumnsDefinition(cd =>
                                        {
                                            cd.ConstantColumn(90);   // etiquetas más compactas
                                            cd.RelativeColumn(2);   // contenido más estrecho
                                        });

                                        // HEADER
                                        tbl.Cell().ColumnSpan(2).Element(c => c
                                            .Border(0.8f)
                                            .BorderColor(Colors.Black)
                                            .Background(Colors.Black)
                                            .Padding(3)
                                            .AlignCenter()
                                            .Text("ATENCIÓN / CONDICIONES")
                                            .FontColor(Colors.White)
                                            .Bold()
                                        );

                                        // Fila 1
                                        tbl.Cell().Element(c =>
                                            c.Background("#D9D9D9")
                                             .Border(0.8f)
                                             .BorderColor(Colors.Grey.Darken2)
                                             .Padding(3)
                                             .Text("Atendido por:")
                                             .Bold()
                                        );
                                        tbl.Cell().Element(c =>
                                            c.Border(0.8f)
                                             .BorderColor(Colors.Grey.Darken2)
                                             .Padding(3)
                                             .Text(atendidoPor)
                                        );

                                        // Fila 2
                                        tbl.Cell().Element(c =>
                                            c.Background("#D9D9D9")
                                             .Border(0.8f)
                                             .BorderColor(Colors.Grey.Darken2)
                                             .Padding(3)
                                             .Text("Forma de pago:")
                                             .Bold()
                                        );
                                        tbl.Cell().Element(c =>
                                            c.Border(0.8f)
                                             .BorderColor(Colors.Grey.Darken2)
                                             .Padding(3)
                                             .Text(formaPago)
                                        );

                                        // Fila 3
                                        tbl.Cell().Element(c =>
                                            c.Background("#D9D9D9")
                                             .Border(0.8f)
                                             .BorderColor(Colors.Grey.Darken2)
                                             .Padding(3)
                                             .Text("Tiempo de entrega:")
                                             .Bold()
                                        );
                                        tbl.Cell().Element(c =>
                                            c.Border(0.8f)
                                             .BorderColor(Colors.Grey.Darken2)
                                             .Padding(3)
                                             .Text(tiempoEntrega)
                                        );
                                    });
                                });


                                row.RelativeColumn(0.5f);


                                row.ConstantColumn(200).Element(right =>
                                {
                                    right.PaddingLeft(4).Element(box =>
                                    {
                                        box.Table(tbl =>
                                        {
                                            tbl.ColumnsDefinition(cd =>
                                            {
                                                cd.RelativeColumn(1);  // titulo
                                                cd.RelativeColumn(1);  // precio
                                            });

                                            // TÍTULO (NEGRO, TEXTO BLANCO)
                                            tbl.Cell().Element(c => c
                                                .Border(0.8f)
                                                .BorderColor(Colors.Black)
                                                .Background(Colors.Black)
                                                .Padding(4)
                                                .AlignCenter()
                                                .Text("IMPORTE TOTAL")
                                                .FontColor(Colors.White)
                                                .Bold()
                                            );

                                            // PRECIO (DERECHA)
                                            tbl.Cell().Element(c => c
                                                .Border(0.8f)
                                                .BorderColor(Colors.Grey.Darken2)
                                                .Padding(4)
                                                .AlignRight()
                                                .Text($"S/ {total:N2}")
                                                .Bold()
                                            );
                                        });
                                    });
                                });
                            });
                        });

                        col.Item().AlignCenter()
                            .Text("\n\nOPERACIÓN SUJETA AL SISTEMA DE PAGO DE OBLIGACIONES TRIBUTARIAS CON EL GOBIERNO CENTRAL Banco de la Nación CTA DE TRACCIONES: 00741755262\nSERVICIO MANO DE OBRA NO INCLUYE IGV")
                            .FontSize(7.5f);
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

            var pdfBytes = document.GeneratePdf();
            Response.Headers["Content-Disposition"] = "inline; filename=cotizacion.pdf";
            return new FileContentResult(pdfBytes, "application/pdf");
        }
    }
}
