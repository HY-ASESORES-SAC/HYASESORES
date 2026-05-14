using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using proyectoIngSoft.Data;
using proyectoIngSoft.Models;

namespace proyectoIngSoft.Controllers
{
    public class DocumentoMedicoController : Controller
    {
        private readonly ILogger<DocumentoMedicoController> _logger;
        private readonly ApplicationDbContext _context;

        public DocumentoMedicoController(ILogger<DocumentoMedicoController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(int descansoId)
        {
            var documentos = _context.DocumentosMedicos
                                    .Where(d => d.DescansoId == descansoId)
                                    .ToList();

            ViewBag.DescansoId = descansoId; // guardamos para usar en la vista
            return View(documentos);
        }

        [HttpPost]
        public IActionResult Subir(List<IFormFile> archivos, int descansoId)
        {
            if (archivos == null || !archivos.Any())
            {
                TempData["Message"] = "No se seleccionaron archivos.";
                return RedirectToAction("Index", new { descansoId });
            }
            

            foreach (var archivo in archivos)
            {
                using (var stream = new MemoryStream())
                {
                    archivo.CopyTo(stream);
                    var doc = new DocumentoMedico
                    {
                        Nombre = archivo.FileName,
                        Tamaño = archivo.Length,
                        FechaSubida = DateTime.UtcNow,
                        Archivo = stream.ToArray(),
                        DescansoId = descansoId
                    };

                    _context.DocumentosMedicos.Add(doc);
                }
            }

            _context.SaveChanges();
            TempData["Message"] = $"{archivos.Count} archivo(s) enviado(s) exitosamente.";
            return RedirectToAction("Index", new { descansoId});
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}