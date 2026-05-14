using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyectoIngSoft.Data;
using proyectoIngSoft.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace proyectoIngSoft.Controllers
{
    public class SimulacionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SimulacionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Simulaciones/CrearNotificacion
        public async Task<IActionResult> CrearNotificacion(string busqueda)
        {
            IQueryable<User> query = _context.DbSetUser
                .Where(u => u.Rol == "Trabajador" || u.Rol == "Asistente de Trabajador");

            if (!string.IsNullOrEmpty(busqueda))
            {
                busqueda = busqueda.ToLower();
                query = query.Where(u =>
                    u.Dni.ToLower().Contains(busqueda) ||
                    u.Username.ToLower().Contains(busqueda) ||
                    u.Apellidos.ToLower().Contains(busqueda));
            }

            var trabajadores = await query.OrderBy(u => u.Username).ToListAsync();
            ViewBag.Busqueda = busqueda;
            return View(trabajadores);
        }

        // POST: /Simulaciones/CrearNotificacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearNotificacion(string tipoNotificacion, string mensaje, string[] trabajadoresSeleccionados)
        {
            if (trabajadoresSeleccionados == null || trabajadoresSeleccionados.Length == 0)
            {
                TempData["Error"] = "No se seleccionó ningún trabajador.";
            }
            else
            {
                var remitente = User.Identity?.Name ?? "oskar@oskar"; // quien envía
                foreach (var idStr in trabajadoresSeleccionados)
                {
                    if (int.TryParse(idStr, out int id))
                    {
                        var usuario = await _context.DbSetUser.FindAsync(id);
                        if (usuario != null)
                        {
                           var noti = new NotificacionSimulada
{
    De = remitente,
    Para = usuario.Email,
    Tipo = tipoNotificacion,
    Mensaje = mensaje,
    Fecha = DateTime.UtcNow  // ✅ usar UTC
};
                            _context.DbSetNotificacionSimulada.Add(noti);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Exito"] = $"Se notificó a {trabajadoresSeleccionados.Length} trabajador(es) correctamente.";
            }

            var trabajadores = await _context.DbSetUser
                .Where(u => u.Rol == "Trabajador" || u.Rol == "Asistente de Trabajador")
                .OrderBy(u => u.Username)
                .ToListAsync();

            return View(trabajadores);
        }

        // GET: /Simulaciones/MisNotificaciones
        public async Task<IActionResult> MisNotificaciones()
{
    // Obtener el email del usuario logueado (desde sesión)
    var userEmail = HttpContext.Session.GetString("User");

    if (string.IsNullOrEmpty(userEmail))
    {
        TempData["Error"] = "No se pudo identificar al usuario actual.";
        return RedirectToAction("Login", "Auth");
    }

    // Traer solo las notificaciones destinadas a este usuario
    var notificaciones = await _context.DbSetNotificacionSimulada
        .Where(n => n.Para.ToLower() == userEmail.ToLower())
        .OrderByDescending(n => n.Fecha)
        .ToListAsync();

    return View(notificaciones);
}

    }
}
