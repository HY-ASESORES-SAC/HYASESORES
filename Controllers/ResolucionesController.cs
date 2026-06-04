using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyectoIngSoft.Data;
using proyectoIngSoft.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace proyectoIngSoft.Controllers
{
    public class ResolucionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResolucionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Vista principal con datos de base de datos
        public async Task<IActionResult> Index()
        {
            int anioActual = DateTime.Now.Year;

            var descansos = await _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.TipoDescanso)
                .Where(d => d.FechaSolicitud.Year == anioActual)
                .ToListAsync();

            var reporteMensual = descansos
                .GroupBy(d => d.FechaSolicitud.Month)
                .Select(g => new ReporteMesViewModel
                {
                    Mes = new DateTime(anioActual, g.Key, 1).ToString("MMMM"),
                    Total = g.Count(),
                    Subsidios = g.Count(d => d.EstadoSubsidioA == "Descanso Activo"),
                    DescansosMedicos = g.Count(d => d.EstadoESSALUD == "Válido"),
                    Aprobados = g.Count(d => d.EstadoSubsidioJ == "Aprobado"),
                    Rechazados = g.Count(d => d.EstadoSubsidioJ == "Rechazado")
                })
                .OrderBy(m => DateTime.ParseExact(m.Mes, "MMMM", new System.Globalization.CultureInfo("es-ES")))
                .ToList();

            var total = reporteMensual.Sum(x => x.Total);
            var aprobados = reporteMensual.Sum(x => x.Aprobados);
            var rechazados = reporteMensual.Sum(x => x.Rechazados);
            var promedioMensual = reporteMensual.Count > 0 ? total / reporteMensual.Count : 0;

            ViewBag.Total = total;
            ViewBag.Aprobados = aprobados;
            ViewBag.Rechazados = rechazados;
            ViewBag.Promedio = promedioMensual;
            ViewBag.PorcentajeAprobados = total > 0 ? Math.Round((decimal)aprobados / total * 100, 1) : 0;
            ViewBag.PorcentajeRechazados = total > 0 ? Math.Round((decimal)rechazados / total * 100, 1) : 0;
            ViewBag.FechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            return View(reporteMensual);
        }

        // ✅ Exportar a PDF sin errores de FontFamily
        public async Task<IActionResult> ExportarResolucionesPDF()
        {
            var resoluciones = await _context.DocumentosMedicos
                .Include(d => d.Descanso)
                .ThenInclude(d => d.User)
                .OrderByDescending(d => d.IdDocumento)
                .ToListAsync();

            using (MemoryStream ms = new MemoryStream())
            {
                // Crear documento PDF
                Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // 🔹 Fuentes sin usar FontFamily (versión compatible)
                Font fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                Font fontSub = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10);
                Font fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                // 🔹 Título principal
                var titulo = new Paragraph("REPORTE DE RESOLUCIONES MÉDICAS 2025", fontTitulo)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                doc.Add(titulo);

                // 🔹 Fecha de generación
                var fecha = new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fontSub)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingAfter = 10
                };
                doc.Add(fecha);

                // 🔹 Crear tabla
                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                table.AddCell(new Phrase("N°", fontNormal));
                table.AddCell(new Phrase("Trabajador", fontNormal));
                table.AddCell(new Phrase("Tipo Descanso", fontNormal));
                table.AddCell(new Phrase("Fecha Solicitud", fontNormal));

                int i = 1;
                foreach (var r in resoluciones)
                {
                    table.AddCell(new Phrase(i.ToString(), fontNormal));
                    table.AddCell(new Phrase($"{r.Descanso.User.Username} {r.Descanso.User.Apellidos}", fontNormal));
                    table.AddCell(new Phrase(r.Descanso.TipoDescanso?.Nombre ?? "-", fontNormal));
                    table.AddCell(new Phrase(r.Descanso.FechaSolicitud.ToString("dd/MM/yyyy"), fontNormal));
                    i++;
                }

                doc.Add(table);
                doc.Close();
                writer.Close();

                // Descargar con nombre dinámico
                string fileName = $"Resoluciones_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
                return File(ms.ToArray(), "application/pdf", fileName);
            }
        }
    }

    // ✅ ViewModel
    public class ReporteMesViewModel
    {
        public string Mes { get; set; }
        public int Total { get; set; }
        public int Subsidios { get; set; }
        public int DescansosMedicos { get; set; }
        public int Aprobados { get; set; }
        public int Rechazados { get; set; }
    }
}
