using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyectoIngSoft.Data;
using proyectoIngSoft.Models;
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

        public IActionResult Index()
        {
            // Obtener el ID del usuario logueado (según Identity)
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 🔹 Si el usuario no está logueado o no hay claim, mostramos todas (modo desarrollo)
            if (string.IsNullOrEmpty(userIdClaim))
            {
                var todas = _context.Notifications
                    .OrderByDescending(n => n.Fecha)
                    .ToList();

                ViewBag.NombreUsuario = "Modo Prueba (sin login)";
                return View("Index", todas);
            }

            // Buscar al usuario en T_Usuarios para obtener su IdUser
            var usuario = _context.DbSetUser
                .FirstOrDefault(u => u.Email == User.Identity.Name);

            if (usuario == null)
            {
                ViewBag.NombreUsuario = "Usuario no encontrado";
                return View("Index", new List<Notification>());
            }

            // 🔹 Cargar solo las notificaciones de este usuario
            var notificaciones = _context.Notifications
                .Where(n => n.UserId == usuario.IdUser.ToString())
                .OrderByDescending(n => n.Fecha)
                .ToList();

            ViewBag.NombreUsuario = $"{usuario.Username} {usuario.Apellidos}";

            return View("Index", notificaciones);
        }
    }
}
