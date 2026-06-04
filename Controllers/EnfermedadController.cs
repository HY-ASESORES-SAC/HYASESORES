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
        [ValidateAntiForgeryToken]
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

                // 3. Crear Descanso (usar conversión segura de fechas)
                var descanso = new Descanso
                {
                    UserId = user.IdUser,               // FK a T_Usuarios
                    TipoDescansoId = 2,                 // 2 = Enfermedad (ajustar si su dominio usa otro id)
                    FechaSolicitud = DateTime.UtcNow,
                    FechaIni = ConvertToUtc(model.FechaIni),
                    FechaFin = ConvertToUtc(model.FechaFin),

                    EnfermedadId = model.IdEnfermedad   // FK a Enfermedad recién creado
                };

                _context.DbSetDescanso.Add(descanso);
                _context.SaveChanges();

                ViewData["Message"] = "Enfermedad registrada con éxito";
                return RedirectToAction("Index", "DocumentoMedico", new { descansoId = descanso.IdDescanso });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar Enfermedad");
                ViewData["Message"] = "Error al registrar: " + ex.Message;
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