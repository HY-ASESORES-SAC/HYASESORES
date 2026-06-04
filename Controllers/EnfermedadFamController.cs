using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using proyectoIngSoft.Data;
using proyectoIngSoft.Models;
using proyectoIngSoft.Helpers;

namespace proyectoIngSoft.Controllers
{
   
    public class EnfermedadFamController : Controller
    {
        private readonly ILogger<EnfermedadFamController> _logger;
        private readonly ApplicationDbContext _context;

        public EnfermedadFamController(ILogger<EnfermedadFamController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
       
        public IActionResult Registrar(EnfermedadFam model, List<IFormFile> archivos)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                _logger.LogWarning("ModelState inválido en EnfermedadFam: {Errors}", errors);
                ViewData["Message"] = "Datos de entrada no válidos: " + errors;
                return View("Index", model);
            }

            try
            {
                // 1. Obtener usuario actual
                var user = UserHelper.GetCurrentUser(HttpContext, _context);
                if (user == null)
                {
                    ViewData["Message"] = "No hay usuario autenticado. Por favor inicie sesión.";
                    return RedirectToAction("Login", "Auth");
                }

                // 2. Guardar Enfermedad Familiar
                _context.DbSetEnfermedadF.Add(model);
                _context.SaveChanges();

                // 3. Crear Descanso
                var descanso = new Descanso
                {
                    UserId = user.IdUser,               // FK a T_Usuarios
                    TipoDescansoId = 5,                 // 5 = Enfermedad Familiar
                    FechaSolicitud = DateTime.UtcNow,
                    FechaIni = DateTime.SpecifyKind(model.FechaIni.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                    FechaFin = DateTime.SpecifyKind(model.FechaFin.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                    EnfermedadFamId = model.IdEnfermedadFam,
                    EstadoProcesado = "Pendiente"       // Inicializar EstadoProcesado
                };

                _context.DbSetDescanso.Add(descanso);
                _context.SaveChanges();

                // 4. Guardar archivos adjuntos
                if (archivos != null && archivos.Any())
                {
                    foreach (var archivo in archivos)
                    {
                        if (archivo.Length > 0)
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
                                    DescansoId = descanso.IdDescanso
                                };
                                _context.DocumentosMedicos.Add(doc);
                            }
                        }
                    }
                    _context.SaveChanges();
                }

                _logger.LogInformation("EnfermedadFam registrada exitosamente con {Count} archivos. Descanso ID: {DescansoId}", archivos?.Count ?? 0, descanso.IdDescanso);
                return RedirectToAction("Index", "ValidarDatos");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar enfermedad familiar");
                ViewData["Message"] = "Error al registrar: " + ex.Message;
                return View("Index", model);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}