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

    public class AccidenteController : Controller
    {
        private readonly ILogger<AccidenteController> _logger;
        private readonly ApplicationDbContext _context;

        public AccidenteController(ILogger<AccidenteController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /Accidente/Index
        public IActionResult Index()
        {
           
            return View();
        }

        [HttpPost]
        public IActionResult Registrar(Accidente model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                _logger.LogWarning("ModelState inválido en Accidente: {Errors}", errors);
                ViewData["Message"] = "Datos no válidos: " + errors;
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

                // 2. Guardar Accidente
                _context.DbSetAccidente.Add(model);
                _context.SaveChanges();
                // 3. Crear Descanso
                var descanso = new Descanso
                {
                    UserId = user.IdUser,               // FK a T_Usuarios
                    TipoDescansoId = 6,                 // 6 = Accidente
                    FechaSolicitud = DateTime.UtcNow,
                    FechaIni = DateTime.SpecifyKind(model.FechaIni.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                    FechaFin = DateTime.SpecifyKind(model.FechaFin.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                    AccidenteId = model.IdAccidente,
                    EstadoProcesado = "Pendiente"       // Inicializar EstadoProcesado
                };

                _context.DbSetDescanso.Add(descanso);
                _context.SaveChanges();

                _logger.LogInformation("Accidente registrado exitosamente. Descanso ID: {DescansoId}", descanso.IdDescanso);
                return RedirectToAction("Index", "DocumentoMedico", new { descansoId = descanso.IdDescanso });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar accidente");
                ViewData["Message"] = "Error al registrar: " + ex.Message;
                return View("Index", model);
            }
        }
        
        
    }
}