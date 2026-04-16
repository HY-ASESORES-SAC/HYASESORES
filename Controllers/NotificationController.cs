using Microsoft.AspNetCore.Mvc;
using proyectoIngSoft.Data;
using proyectoIngSoft.Models;
using System.Linq;

namespace proyectoIngSoft.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Lista todas las notificaciones del usuario actual (ejemplo con User.Identity.Name)
        public IActionResult Index()
        {
            var userId = User.Identity?.Name; // O usa Id real de Identity
            var notificaciones = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.Fecha)
                .ToList();

            return View(notificaciones);
        }
        
        

        // 🔹 Detalles de una notificación
        public IActionResult Details(int id)
        {
            var notificacion = _context.Notifications.FirstOrDefault(n => n.Id == id);
            if (notificacion == null)
                return NotFound();

            return PartialView("_NotificationDetail", notificacion);
        }

        // 🔹 Crear (cuando admin envía notificación a un usuario)
        [HttpPost]
        public IActionResult Create(Notification model)
        {
            if (ModelState.IsValid)
            {
                model.Fecha = DateTime.Now;
                _context.Notifications.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}
