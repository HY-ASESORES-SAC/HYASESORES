using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyectoIngSoft.Data;
using proyectoIngSoft.Models;
using proyectoIngSoft.Helpers;
using System.Linq;
using System.Security.Claims;

namespace proyectoIngSoft.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Endpoint de depuración
        public IActionResult Debug()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name;
            var usuario = _context.DbSetUser.FirstOrDefault(u => u.Email == userName);

            var debug = new
            {
                UserIdClaim = userIdClaim ?? "NULL",
                UserName = userName ?? "NULL",
                UsuarioEncontrado = usuario != null,
                IdUser = usuario?.IdUser.ToString() ?? "NULL",
                TodasLasNotificaciones = _context.Notifications.Select(n => new { n.Id, n.UserId, n.Titulo }).ToList()
            };

            return Json(debug);
        }

        public IActionResult Index()
        {
            // Obtener el usuario actual usando el helper
            var usuario = UserHelper.GetCurrentUser(HttpContext, _context);

            // Si no hay usuario autenticado, mostrar todas (modo desarrollo)
            if (usuario == null)
            {
                var todas = _context.Notifications
                    .OrderByDescending(n => n.Fecha)
                    .ToList();

                ViewBag.NombreUsuario = "Modo Prueba (sin login)";
                return View("Index", todas);
            }

            // Cargar solo las notificaciones de este usuario
            var notificaciones = _context.Notifications
                .Where(n => n.UserId == usuario.IdUser.ToString())
                .OrderByDescending(n => n.Fecha)
                .ToList();

            ViewBag.NombreUsuario = $"{usuario.Username} {usuario.Apellidos}";

            return View("Index", notificaciones);
        }
    }
}
