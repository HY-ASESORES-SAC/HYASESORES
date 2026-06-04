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
    public class EnfermedadController : Controller
    {
        private readonly ILogger<EnfermedadController> _logger;
        private readonly ApplicationDbContext _context;

        public EnfermedadController(ILogger<EnfermedadController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
    
            return View();
        }

       
        [HttpPost]
        public IActionResult Registrar(Enfermedad model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Datos no válidos";
                return View("Index");
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

                // 2. Guardar Enfermedad
                _context.DbSetEnfermedad.Add(model);
                _context.SaveChanges();

                // 3. Crear Descanso
                var descanso = new Descanso
                {
                    UserId = user.IdUser,               // FK a T_Usuarios
                    TipoDescansoId = 1,                 // 1 = Accidente
                    FechaSolicitud = DateTime.UtcNow,
                    FechaIni = DateTime.SpecifyKind(model.FechaIni.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                    FechaFin = DateTime.SpecifyKind(model.FechaFin.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),

                    EnfermedadId = model.IdEnfermedad   // FK al Accidente recién creado
                };

                _context.DbSetDescanso.Add(descanso);
                _context.SaveChanges();

                ViewData["Message"] = "Accidente registrado con éxito";
                return RedirectToAction("Index", "DocumentoMedico", new { descansoId = descanso.IdDescanso });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar Accidente");
                ViewData["Message"] = "Error al registrar: " + ex.Message;
            }

            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}