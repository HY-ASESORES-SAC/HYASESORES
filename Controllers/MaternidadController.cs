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
  
    public class MaternidadController : Controller
    {
        private readonly ILogger<MaternidadController> _logger;
        private readonly ApplicationDbContext _context;

        public MaternidadController(ILogger<MaternidadController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
       
        public IActionResult Registrar(Maternidad model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                _logger.LogWarning("ModelState inválido en Maternidad: {Errors}", errors);
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

                // 2. Guardar Maternidad
                _context.DbSetMaternidad.Add(model);
                _context.SaveChanges();

                // 3. Crear Descanso
                var descanso = new Descanso
                {
                    UserId = user.IdUser,               // FK a T_Usuarios
                    TipoDescansoId = 2,                 // 2 = Maternidad
                    FechaSolicitud = DateTime.UtcNow,
                    FechaIni = DateTime.SpecifyKind(model.FechaIni.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                    FechaFin = DateTime.SpecifyKind(model.FechaFin.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                    MaternidadId = model.IdMater,
                    EstadoProcesado = "Pendiente"       // Inicializar EstadoProcesado
                };

                _context.DbSetDescanso.Add(descanso);
                _context.SaveChanges();

                _logger.LogInformation("Maternidad registrada exitosamente. Descanso ID: {DescansoId}", descanso.IdDescanso);
                return RedirectToAction("Index", "DocumentoMedico", new { descansoId = descanso.IdDescanso });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar maternidad");
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