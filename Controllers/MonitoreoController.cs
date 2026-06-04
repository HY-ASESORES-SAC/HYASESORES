using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyectoIngSoft.Data;
using proyectoIngSoft.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace proyectoIngSoft.Controllers
{
    public class MonitoreoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MonitoreoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ INDEX (vista principal de monitoreo)
        public async Task<IActionResult> Index(string tipo, string busqueda)
        {
            IQueryable<Descanso> query = _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.TipoDescanso)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tipo) && tipo.ToLower() == "subsidio")
            {
                query = query.Where(d => d.EstadoSubsidioA == "Descanso Activo");
            }

            if (!string.IsNullOrEmpty(busqueda))
            {
                busqueda = busqueda.ToLower();
                query = query.Where(d =>
                    d.User.Username.ToLower().Contains(busqueda) ||
                    d.User.Apellidos.ToLower().Contains(busqueda) ||
                    d.User.Email.ToLower().Contains(busqueda) ||
                    d.User.Dni.ToLower().Contains(busqueda));
            }

            var lista = await query
                .Select(d => new MonitoreoViewModel
                {
                    Nombre = d.User.Username + " " + d.User.Apellidos,
                    TipoSubsidio = d.TipoDescanso.Nombre,
                    FechaInicio = d.FechaIni,
                    FechaFin = d.FechaFin,
                    Dias = (int)(d.FechaFin - d.FechaIni).TotalDays + 1,
                    PagoPorDia = 85.5m,
                    Total = ((int)(d.FechaFin - d.FechaIni).TotalDays + 1) * 85.5m,
                    Estado = d.EstadoSubsidioJ
                })
                .OrderByDescending(x => x.FechaInicio)
                .ToListAsync();

            ViewBag.TipoSeleccionado = tipo;
            ViewBag.Busqueda = busqueda;

            return View(lista);
        }

        // ✅ NUEVA VISTA HU16
        [HttpGet]
        public async Task<IActionResult> HU16()
        {
            var descansos = await _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.TipoDescanso)
                .OrderByDescending(d => d.FechaIni)
                .Select(d => new MonitoreoViewModel
                {
                    Nombre = d.User.Username + " " + d.User.Apellidos,
                    TipoSubsidio = d.TipoDescanso.Nombre,
                    FechaInicio = d.FechaIni,
                    FechaFin = d.FechaFin,
                    Dias = (int)(d.FechaFin - d.FechaIni).TotalDays + 1,
                    PagoPorDia = 85.5m,
                    Total = ((int)(d.FechaFin - d.FechaIni).TotalDays + 1) * 85.5m,
                    Estado = d.EstadoSubsidioJ
                })
                .ToListAsync();

            return View(descansos ?? new List<MonitoreoViewModel>());
        }

        // ✅ DETALLES de un descanso
        public async Task<IActionResult> Detalles(int id)
        {
            var descanso = await _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.DocumentosMedicos)
                .Include(d => d.TipoDescanso)
                .FirstOrDefaultAsync(d => d.IdDescanso == id);

            if (descanso == null)
                return NotFound();

            return View(descanso);
        }

        // ✅ VER DOCUMENTO MÉDICO (PDF)
        public async Task<IActionResult> VerDocumento(int id)
        {
            var documento = await _context.DocumentosMedicos
                .Include(d => d.Descanso)
                .FirstOrDefaultAsync(d => d.IdDocumento == id);

            if (documento == null || documento.Archivo == null)
                return NotFound();

            return File(documento.Archivo, "application/pdf", documento.Nombre);
        }

        // ✅ EXPORTAR HU16 A PDF
        public IActionResult ExportarPDF_HU16()
        {
            var descansos = _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.TipoDescanso)
                .OrderByDescending(d => d.FechaIni)
                .Select(d => new MonitoreoViewModel
                {
                    Nombre = d.User.Username + " " + d.User.Apellidos,
                    TipoSubsidio = d.TipoDescanso.Nombre,
                    FechaInicio = d.FechaIni,
                    FechaFin = d.FechaFin,
                    Dias = (int)(d.FechaFin - d.FechaIni).TotalDays + 1,
                    PagoPorDia = 85.5m,
                    Total = ((int)(d.FechaFin - d.FechaIni).TotalDays + 1) * 85.5m,
                    Estado = d.EstadoSubsidioJ
                })
                .ToList();

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Título
                var titulo = new Paragraph("Informe HU16 - Monitoreo")
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                };
                doc.Add(titulo);

                // Tabla
                PdfPTable tabla = new PdfPTable(6) { WidthPercentage = 100 };
                tabla.SetWidths(new float[] { 3, 2, 2, 1, 2, 2 });

                // Encabezados
                tabla.AddCell("Nombre");
                tabla.AddCell("Tipo Subsidio");
                tabla.AddCell("Fecha Inicio");
                tabla.AddCell("Días");
                tabla.AddCell("Pago por Día");
                tabla.AddCell("Total");

                // Filas
                foreach (var d in descansos)
                {
                    tabla.AddCell(d.Nombre);
                    tabla.AddCell(d.TipoSubsidio);
                    tabla.AddCell(d.FechaInicio.ToString("dd/MM/yyyy"));
                    tabla.AddCell(d.Dias.ToString());
                    tabla.AddCell(d.PagoPorDia.ToString("C"));
                    tabla.AddCell(d.Total.ToString("C"));
                }

                doc.Add(tabla);
                doc.Close();

                byte[] archivoBytes = ms.ToArray();
                return File(archivoBytes, "application/pdf", "Informe_HU16.pdf");
            }
        }
    }

    // ✅ VIEWMODEL PARA VISTA HU16 Y INDEX
    public class MonitoreoViewModel
    {
        public string Nombre { get; set; }
        public string TipoSubsidio { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int Dias { get; set; }
        public decimal PagoPorDia { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
    }
}
