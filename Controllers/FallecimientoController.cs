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
  
    public class FallecimientoController : Controller
    {
        private readonly ILogger<FallecimientoController> _logger;
        private readonly ApplicationDbContext _context;

        public FallecimientoController(ILogger<FallecimientoController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(Fallecimiento model)
        {
            // Debug: Log all form values
            _logger.LogInformation("=== INICIO REGISTRO FALLECIMIENTO ===");
            _logger.LogInformation("NombreFallec: {NombreFallec}", model.NombreFallec);
            _logger.LogInformation("Parentesco: {Parentesco}", model.Parentesco);
            _logger.LogInformation("FechaIni: {FechaIni}", model.FechaIni);
            _logger.LogInformation("FechaFin: {FechaFin}", model.FechaFin);
            _logger.LogInformation("FechaComun: {FechaComun}", model.FechaComun);
            _logger.LogInformation("LugarSep: {LugarSep}", model.LugarSep);
            _logger.LogInformation("Traslado: {Traslado}", model.Traslado);
            _logger.LogInformation("ModelState.IsValid: {IsValid}", ModelState.IsValid);
            
            if (!ModelState.IsValid)
            {
                try
                {
                    // 1. Obtener usuario actual
                    var user = UserHelper.GetCurrentUser(HttpContext, _context);
                    if (user == null)
                    {
                        // No hay usuario autenticado
                        ViewData["Message"] = "No hay usuario autenticado. Por favor inicie sesión.";
                        return RedirectToAction("Login", "Auth");
                    }

                    // 2. Guardar Fallecimiento
                    _context.DbSetFallecimiento.Add(model);
                    _context.SaveChanges();

                    // 3. Crear Descanso (usar conversión segura de fechas)
                    var descanso = new Descanso
                    {
                        UserId = user.IdUser,               // FK a T_Usuarios
                        TipoDescansoId = 4,                 // 4 = Fallecimiento Familiar
                        FechaSolicitud = DateTime.UtcNow,
                        FechaIni = DateTime.SpecifyKind(model.FechaIni.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                        FechaFin = DateTime.SpecifyKind(model.FechaFin.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                        FallecimientoId = model.IdFallec
                    };

                _context.DbSetDescanso.Add(descanso);
                _context.SaveChanges();

                    ViewData["Message"] = "Fallecimiento registrado con éxito";
                    return RedirectToAction("Index", "DocumentoMedico", new { descansoId = descanso.IdDescanso });
                }
                catch (Exception ex)
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

                _logger.LogInformation("Fallecimiento registrado exitosamente con {Count} archivos. Descanso ID: {DescansoId}", archivos?.Count ?? 0, descanso.IdDescanso);
                return RedirectToAction("Index", "ValidarDatos");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar fallecimiento");
                ViewData["Message"] = "Error al registrar: " + ex.Message;
                return View("Index", model);
            }
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error");
        }

		// Método auxiliar para convertir fechas a UTC de forma robusta
        private DateTime ConvertToUtc(object dateObj)
        {
            if (dateObj == null)
                throw new ArgumentNullException(nameof(dateObj), "La fecha no puede ser null");

            if (dateObj is DateTime dt)
            {
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }

#if NET6_0_OR_GREATER
            if (dateObj is DateOnly d)
            {
                return DateTime.SpecifyKind(d.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            }
#endif
            var s = dateObj.ToString();
            if (DateTime.TryParse(s, out var parsed))
            {
                return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            }

            throw new InvalidOperationException("Tipo de fecha no soportado: " + dateObj.GetType());
        }
    }
}