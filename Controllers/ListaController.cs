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


        public IActionResult DescansosProlongados()
        {
            var descansos = _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.TipoDescanso)
                .ToList()
                .Where(d =>
                {
                    // Cálculo en días entre FechaInicio y FechaFin
                    var dias = (d.FechaFin - d.FechaIni).TotalDays;
                    return dias > 30;
                })
                .Select(d => new
                {
                    d.IdDescanso,
                    d.User.Dni,
                    Nombre = $"{d.User.Username} {d.User.Apellidos}",
                    Motivo = d.TipoDescanso.Nombre,
                    d.FechaIni,
                    d.FechaFin,
                    Dias = (d.FechaFin - d.FechaIni).TotalDays,
                    d.EstadoSubsidioA,
                    Estado = d.EstadoESSALUD ?? "Descanso Activo"
                })
                .ToList();

            return View("DescansosProlongados", descansos);
        }

      
        [HttpPost]
        public IActionResult ValidarSubsidioA(int id)
        {
            try
            {
                // Usa la DbSet correcta en tu ApplicationDbContext
                var descanso = _context.DbSetDescanso.FirstOrDefault(d => d.IdDescanso == id);
                if (descanso == null)
                    return Json(new { success = false, message = "Descanso no encontrado." });

                // Cambiamos su estado a "Subsidio"
                descanso.EstadoSubsidioA = "Subsidio";

                _context.Update(descanso);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Devuelve el mensaje del error para debug en el frontend (temporal)
                return Json(new { success = false, message = ex.Message });
            }
        }


        public IActionResult DetalleProlongado(int descansoId)
        {
            var descanso = _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.TipoDescanso)
                .Include(d => d.DocumentosMedicos)
                .FirstOrDefault(d => d.IdDescanso == descansoId);

            if (descanso == null) return NotFound();

            return PartialView("_DetalleProlongado", descanso);
        }


        public IActionResult SubsidiosJefe()
        {
            var descansos = _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.TipoDescanso)
                .Where(d => d.EstadoSubsidioA == "Subsidio")
                .Select(d => new
                {
                    d.IdDescanso,
                    d.User.Dni,
                    Nombre = $"{d.User.Username} {d.User.Apellidos}",
                    Motivo = d.TipoDescanso.Nombre,
                    d.FechaIni,
                    d.FechaFin,
                    Dias = (d.FechaFin - d.FechaIni).TotalDays,
                    d.EstadoSubsidioA,
                    d.EstadoSubsidioJ
                })
                .ToList();

                ViewBag.Total = descansos.Count;
                ViewBag.Pendientes = descansos.Count(d => d.EstadoSubsidioJ == "Pendiente" || d.EstadoSubsidioJ == null);
                ViewBag.Aprobados = descansos.Count(d => d.EstadoSubsidioJ == "Aprobado");
                ViewBag.Rechazados = descansos.Count(d => d.EstadoSubsidioJ == "Rechazado");

            return View("SubsidiosJefe", descansos);
        }

        [HttpPost]
        public IActionResult AprobarSubsidioJ(int id)
        {
            var descanso = _context.DbSetDescanso.FirstOrDefault(d => d.IdDescanso == id);
            if (descanso == null)
                return Json(new { success = false, message = "No se encontró el descanso." });

            descanso.EstadoSubsidioJ = "Aprobado";
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult RechazarSubsidioJ(int id)
        {
            var descanso = _context.DbSetDescanso.FirstOrDefault(d => d.IdDescanso == id);
            if (descanso == null)
                return Json(new { success = false, message = "No se encontró el descanso." });

            descanso.EstadoSubsidioJ = "Rechazado";
            _context.SaveChanges();

            return Json(new { success = true });
        }
// ============================
// SUPERVISIÓN Y VALIDACIÓN
// ============================
public IActionResult SupervisionSubsidios()
{
    var descansos = _context.DbSetDescanso
        .Include(d => d.User)
        .Include(d => d.TipoDescanso)
        .Where(d => d.EstadoSubsidioA == "Subsidio" &&
                    (d.EstadoSubsidioJ == "Aprobado" || d.EstadoSubsidioJ == "Rechazado"))
        .Select(d => new SupervisionSubsidioViewModel
        {
            IdDescanso = d.IdDescanso,
            Dni = d.User.Dni,
            Nombre = $"{d.User.Username} {d.User.Apellidos}",
            Motivo = d.TipoDescanso.Nombre,
            FechaIni = d.FechaIni,
            FechaFin = d.FechaFin,
            Dias = (d.FechaFin - d.FechaIni).TotalDays,
            EstadoSubsidioA = d.EstadoSubsidioA,
            EstadoSubsidioJ = d.EstadoSubsidioJ
        })
        .ToList();

    return View("SupervisionSubsidios", descansos);
}


[HttpPost]
public IActionResult EnviarSeleccionados([FromBody] List<int> idsSeleccionados)
{
    if (idsSeleccionados == null || !idsSeleccionados.Any())
        return BadRequest("No se seleccionaron registros.");

    var seleccionados = _context.DbSetDescanso
        .Include(d => d.User)
        .Include(d => d.TipoDescanso)
        .Where(d => idsSeleccionados.Contains(d.IdDescanso))
        .Select(d => new TrabajadorSeleccionadoViewModel
        {
            IdDescanso = d.IdDescanso,
            Dni = d.User.Dni,
            Nombre = $"{d.User.Username} {d.User.Apellidos}",
            Motivo = d.TipoDescanso.Nombre,
            FechaIni = d.FechaIni,
            FechaFin = d.FechaFin,
            Dias = (d.FechaFin - d.FechaIni).TotalDays,
            EstadoSubsidioA = d.EstadoSubsidioA,
            EstadoSubsidioJ = d.EstadoSubsidioJ
        })
        .ToList();

    TempData["Seleccionados"] = System.Text.Json.JsonSerializer.Serialize(seleccionados);
    return Ok();
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
