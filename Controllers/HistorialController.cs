using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyectoIngSoft.Data;
using proyectoIngSoft.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using System.IO;

namespace proyectoIngSoft.Controllers
{    
    public class HistorialController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HistorialController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Historial/Index
        public async Task<IActionResult> Index(string? tipo, string? motivo, string? estado)
        {
            var query = _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.TipoDescanso)
                .Include(d => d.Enfermedad)
                .Include(d => d.Maternidad)
                .Include(d => d.Paternidad)
                .Include(d => d.Accidente)
                .Include(d => d.Fallecimiento)
                .Include(d => d.EnfermedadFam)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(d => d.TipoDescanso.Nombre.ToLower().Contains(tipo.ToLower()));

            if (!string.IsNullOrEmpty(motivo))
            {
                query = query.Where(d =>
                    (d.Enfermedad != null && motivo == "Enfermedad") ||
                    (d.Maternidad != null && motivo == "Maternidad") ||
                    (d.Paternidad != null && motivo == "Paternidad") ||
                    (d.Accidente != null && motivo == "Accidente") ||
                    (d.Fallecimiento != null && motivo == "Fallecimiento") ||
                    (d.EnfermedadFam != null && motivo == "Enfermedad Familiar")
                );
            }

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(d => d.EstadoSubsidioJ.ToLower() == estado.ToLower());

            var lista = await query.ToListAsync();

            // 📊 Calcular los totales de resumen
            var totalRegistros = lista.Count;
            var porTipo = new Dictionary<string, int>
            {
                ["Enfermedad"] = lista.Count(d => d.EnfermedadId.HasValue),
                ["Maternidad"] = lista.Count(d => d.MaternidadId.HasValue),
                ["Paternidad"] = lista.Count(d => d.PaternidadId.HasValue),
                ["Accidente"] = lista.Count(d => d.AccidenteId.HasValue),
                ["Fallecimiento"] = lista.Count(d => d.FallecimientoId.HasValue),
                ["Enfermedad Familiar"] = lista.Count(d => d.EnfermedadFamId.HasValue)
            };

            var estadoESSALUD = new Dictionary<string, int>
            {
                ["Válido"] = lista.Count(d => d.EstadoESSALUD == "Válido"),
                ["En observación"] = lista.Count(d => d.EstadoESSALUD == "En observación"),
                ["En Proceso"] = lista.Count(d => d.EstadoESSALUD == "En Proceso"),
                ["No Válido"] = lista.Count(d => d.EstadoESSALUD == "No Válido")
            };

            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.PorTipo = porTipo;
            ViewBag.EstadoESSALUD = estadoESSALUD;

            return View(lista);
        }

        // POST: Exportar PDF
        [HttpPost]
        public async Task<IActionResult> ExportarPDF()
        {
            var descansos = await _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.TipoDescanso)
                .ToListAsync();

            using var stream = new MemoryStream();
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
            var font = new XFont("Arial", 10, XFontStyle.Regular);

            int y = 40;
            gfx.DrawString("Historial de Descansos Médicos y Subsidios", fontTitle, XBrushes.Black,
                new XRect(0, y, page.Width, page.Height), XStringFormats.TopCenter);
            y += 40;

            foreach (var d in descansos)
            {
                var dias = (d.FechaFin - d.FechaIni).TotalDays;

                string text = $"ID: {d.IdDescanso} | Empleado: {d.User?.NombreCompleto} | " +
                            $"Tipo: {d.TipoDescanso?.Nombre} | Motivo: {GetMotivo(d)} | " +
                            $"Estado: {d.EstadoSubsidioJ} | Días: {dias}";

                gfx.DrawString(text, font, XBrushes.Black,
                    new XRect(40, y, page.Width - 80, page.Height), XStringFormats.TopLeft);

                y += 18;
                if (y > page.Height - 60)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            document.Save(stream, false);
            return File(stream.ToArray(), "application/pdf", "HistorialDescansos.pdf");
        }
        
        private string GetMotivo(Descanso d)
        {
            if (d.EnfermedadId.HasValue) return "Enfermedad";
            if (d.MaternidadId.HasValue) return "Maternidad";
            if (d.PaternidadId.HasValue) return "Paternidad";
            if (d.AccidenteId.HasValue) return "Accidente";
            if (d.FallecimientoId.HasValue) return "Fallecimiento";
            if (d.EnfermedadFamId.HasValue) return "Enfermedad Familiar";
            return "-";
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}