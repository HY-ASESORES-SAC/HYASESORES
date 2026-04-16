using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using proyectoIngSoft.Data;
using proyectoIngSoft.Models;

namespace proyectoIngSoft.Controllers
{
    public class ListaController : Controller
    {
        private readonly ILogger<ListaController> _logger;
        private readonly ApplicationDbContext _context;

        public ListaController(ILogger<ListaController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ============================
        // LISTA GENERAL DE SOLICITUDES
        // ============================
        public IActionResult Index()
        {
            var lista = _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.TipoDescanso)
                .Select(d => new Lista
                {
                    Username = d.User.Username,
                    Apellidos = d.User.Apellidos,
                    Dni = d.User.Dni,
                    Observaciones = d.TipoDescanso.Nombre,
                    FechaSolicitud = d.FechaSolicitud,
                    Estado = "En Proceso",
                    IdUser = d.User.IdUser,
                    IdDescanso = d.IdDescanso
                })
                .ToList();

            return View("Index", lista);
        }

        // =======================================
        // DETALLE DE UNA SOLICITUD DE DESCANSO
        // =======================================
        public IActionResult DetalleDescanso(int descansoId)
        {
            var descanso = _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.TipoDescanso)
                .Include(d => d.Accidente)
                .Include(d => d.Enfermedad)
                .Include(d => d.EnfermedadFam)
                .Include(d => d.Fallecimiento)
                .Include(d => d.Maternidad)
                .Include(d => d.Paternidad)
                .Include(d => d.DocumentosMedicos)
                .FirstOrDefault(d => d.IdDescanso == descansoId);

            if (descanso == null) return NotFound();

            return PartialView("_DetalleDescanso", descanso);
        }

        // ==========================================
        // ENVIAR OBSERVACIÓN Y MOSTRAR CONFIRMACIÓN
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> EnviarObservacion(int descansoId, string mensaje)
        {
            var descanso = await _context.DbSetDescanso
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.IdDescanso == descansoId);

            if (descanso == null)
                return NotFound("No se encontró la solicitud de descanso.");

            // Crear la notificación
            var notificacion = new Notification
            {
                UserId = descanso.User.IdUser.ToString(),
                Titulo = "Solicitud en observación",
                Mensaje = string.IsNullOrEmpty(mensaje)
                    ? "Tu solicitud de descanso médico está en observación. Por favor revisa los detalles."
                    : mensaje,
                Estado = "En Observación",
                Fecha = DateTime.UtcNow,
                Detalle = $"Solicitud con ID {descanso.IdDescanso} requiere revisión.",
                DocumentoAdjuntos = new List<string>()
            };

            _context.Notifications.Add(notificacion);
            await _context.SaveChangesAsync();

            // ✅ Mostramos respuesta directa en HTML sin usar vista
            var html = $@"
                <html>
                    <head>
                        <meta charset='UTF-8'>
                        <title>Notificación enviada</title>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                background-color: #f9f9f9;
                                text-align: center;
                                padding-top: 100px;
                            }}
                            .card {{
                                display: inline-block;
                                background: white;
                                padding: 40px 60px;
                                border-radius: 15px;
                                box-shadow: 0 0 10px rgba(0,0,0,0.1);
                            }}
                            .btn {{
                                display: inline-block;
                                margin-top: 20px;
                                padding: 10px 20px;
                                background-color: #007bff;
                                color: white;
                                border-radius: 5px;
                                text-decoration: none;
                            }}
                            .btn:hover {{
                                background-color: #0056b3;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='card'>
                            <h2 style='color:green;'>✅ Notificación enviada correctamente</h2>
                            <p>Se ha enviado una notificación al trabajador:</p>
                            <h3>{descanso.User.Username} {descanso.User.Apellidos}</h3>
                            <p style='margin-top:20px;'>El trabajador ha sido informado sobre el estado de su solicitud.</p>
                            <a class='btn' href='/Lista'>Volver a la lista</a>
                        </div>
                    </body>
                </html>";

            return Content(html, "text/html");
        }

        // ======================
        // VER DOCUMENTO EN IFRAME
        // ======================
        public IActionResult VerDocumento(int id)
        {
            var documento = _context.DocumentosMedicos
                .AsNoTracking()
                .FirstOrDefault(d => d.IdDocumento == id);

            if (documento == null || documento.Archivo == null || documento.Archivo.Length == 0)
                return NotFound();

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{documento.Nombre}\"";
            return File(documento.Archivo, "application/pdf");
        }

        // ======================
        // MANEJO DE ERRORES
        // ======================
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
