using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using proyectoIngSoft.Data;
using proyectoIngSoft.Models;
using proyectoIngSoft.Models.proyectoIngSoft.Models;

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
        // ============================
            public IActionResult Index()
            {
                var lista = _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.TipoDescanso)
                .Where(d => d.EstadoESSALUD == "En Proceso" || d.EstadoESSALUD == "En Observación")
                .Select(d => new Lista
                {
                    Username = d.User.Username,
                    Apellidos = d.User.Apellidos,
                    Dni = d.User.Dni,
                    Observaciones = d.TipoDescanso.Nombre,
                    FechaSolicitud = d.FechaSolicitud,
                    Estado = d.EstadoESSALUD,
                    IdUser = d.User.IdUser,
                    IdDescanso = d.IdDescanso
                })
                .ToList();

            // Solicitudes procesadas - nueva consulta
                var solicitudesProcesadas = _context.DbSetDescanso
                    .Include(d => d.User)
                    .Include(d => d.TipoDescanso)
                    .Where(d => d.EstadoESSALUD == "Válido" || d.EstadoESSALUD == "No válido")
                    .Select(d => new Lista
                    {
                        Username = d.User.Username,
                        Apellidos = d.User.Apellidos,
                        Dni = d.User.Dni,
                        Observaciones = d.TipoDescanso.Nombre,
                        FechaSolicitud = d.FechaSolicitud,
                        Estado = d.EstadoESSALUD, // Resultado ESSALUD
                        EstadoProcesado = d.EstadoProcesado ?? "Procesado", // NUEVO: Estado de la base de datos
                        IdDescanso = d.IdDescanso
                    })
                    .ToList();

                ViewBag.SolicitudesProcesadas = solicitudesProcesadas;
                ViewBag.CountProcesadas = solicitudesProcesadas.Count;

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

        public IActionResult DetalleDescansoProcesadas(int descansoId)
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

            return PartialView("_DetalleDescansoProcesadas", descanso);
        }



        [HttpPost]
        public IActionResult CambiarEstadoProcesada(int descansoId, string nuevoEstado)
        {
            try
            {
                var descanso = _context.DbSetDescanso.FirstOrDefault(d => d.IdDescanso == descansoId);
                if (descanso == null)
                    return Json(new { success = false, message = "No se encontró la solicitud." });

                // Validar que el estado sea correcto
                if (nuevoEstado != "Aceptado" && nuevoEstado != "Rechazado")
                    return Json(new { success = false, message = "Estado no válido." });

                // USAR LA NUEVA PROPIEDAD EstadoProcesado
                descanso.EstadoProcesado = nuevoEstado;

                _context.DbSetDescanso.Update(descanso);
                _context.SaveChanges();

                return Json(new { success = true, message = $"Estado cambiado a: {nuevoEstado}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
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

            // ✅ CAMBIAR EL ESTADO A "En Observación"
            descanso.EstadoESSALUD = "En Observación";
            _context.DbSetDescanso.Update(descanso);

            // Crear la notificación
            var notificacion = new Notification
            {
                UserId = descanso.User.IdUser.ToString(),
                Titulo = "Solicitud en observación",
                Mensaje = string.IsNullOrEmpty(mensaje)
                    ? "Tu solicitud de descanso médico está en observación. Por favor revisa los detalles."
                    : mensaje,
                Estado = "Observacion",
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



        public IActionResult VerDocumentos(int descansoId)
        {
            var descanso = _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.DocumentosMedicos)
                .FirstOrDefault(d => d.IdDescanso == descansoId);

            if (descanso == null)
                return NotFound();

            return View("VerDocumentos", descanso);
        }
        
        [HttpPost]
        public IActionResult EliminarDocumentos(int descansoId, List<int> documentosIds)
        {
            if (documentosIds == null || documentosIds.Count == 0)
                return RedirectToAction("VerDocumentos", new { descansoId });

            var documentos = _context.DocumentosMedicos
                .Where(d => documentosIds.Contains(d.IdDocumento))
                .ToList();

            if (documentos.Any())
            {
                _context.DocumentosMedicos.RemoveRange(documentos);
                _context.SaveChanges();
            }

            return RedirectToAction("VerDocumentos", new { descansoId });
        }

      
        [HttpPost]
        public JsonResult ValidarSubsidioA(int id)
        {
            var descanso = _context.DbSetDescanso.Find(id);
            if (descanso != null)
            {
                // Random para decidir estado
                Random rnd = new Random();
                string[] estados = { "Subsidio", "Rechazado" };
                string estadoSeleccionado = estados[rnd.Next(estados.Length)];

                // Asignar y guardar
                descanso.EstadoSubsidioA = estadoSeleccionado;
                _context.SaveChanges();

                // Devolver estado real al frontend
                return Json(new { success = true, estado = estadoSeleccionado });
            }

            return Json(new { success = false });
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






        public IActionResult ReporteMensual(int? año)
        {
            año ??= DateTime.Now.Year;
            
            var reporte = GenerarReporteMensual(año.Value);
            
            ViewBag.AñoSeleccionado = año.Value;
            ViewBag.AñosDisponibles = Enumerable.Range(DateTime.Now.Year - 5, 10).ToList(); // Últimos 5 años y próximos 5
            
            return View("ReporteMensual", reporte);
        }

        private List<ReporteMensualViewModel> GenerarReporteMensual(int año)
        {
            var reporte = new List<ReporteMensualViewModel>();

            // Obtener todos los datos del año en una sola consulta
            var datosAño = _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.TipoDescanso)
                .AsEnumerable()
                .Where(d => d.FechaSolicitud.Year == año)
                .ToList();

            for (int mes = 1; mes <= 12; mes++)
            {
                var datosMes = datosAño.Where(d => d.FechaSolicitud.Month == mes).ToList();

                // SUBSIDIOS
                var subsidios = datosMes.Where(d => d.EstadoSubsidioA == "Subsidio" || d.EstadoSubsidioA == "Rechazado").ToList();
                var subsidiosAprobados = subsidios.Count(d => d.EstadoSubsidioA == "Subsidio");
                var subsidiosRechazados = subsidios.Count(d => d.EstadoSubsidioA == "Rechazado");

                // DESCANSO MÉDICOS
                var descansosMedicos = datosMes.Where(d =>
                    (!string.IsNullOrEmpty(d.EstadoProcesado) &&
                    (d.EstadoProcesado == "Aceptado" || d.EstadoProcesado == "Rechazado"))
                ).ToList();

                var descansosAprobados = descansosMedicos.Count(d => d.EstadoProcesado == "Aceptado");
                var descansosRechazados = descansosMedicos.Count(d => d.EstadoProcesado == "Rechazado");

                // Calcular totales
                var totalRevisados = subsidios.Count + descansosMedicos.Count;
                var totalAprobados = subsidiosAprobados + descansosAprobados;
                var totalRechazados = subsidiosRechazados + descansosRechazados;

                // Solo agregar el mes si tiene datos
                if (totalRevisados > 0)
                {
                    reporte.Add(new ReporteMensualViewModel
                    {
                        Año = año,
                        Mes = mes,
                        NombreMes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mes),
                        TotalRevisados = totalRevisados,
                        Subsidios = subsidios.Count,
                        DescansosMedicos = descansosMedicos.Count,
                        Aprobados = totalAprobados,
                        Rechazados = totalRechazados
                    });
                }
            }

            // Ordenar por mes
            return reporte.OrderBy(r => r.Mes).ToList();
        }
        


        public IActionResult DetalleMes(int año, int mes)
{
    var detalleMes = ObtenerDetalleMes(año, mes);
    
    if (detalleMes == null || !detalleMes.Solicitudes.Any())
    {
        TempData["Error"] = "No hay solicitudes revisadas para el mes seleccionado.";
        return RedirectToAction("ReporteMensual", new { año });
    }
    
    return View("DetalleMes", detalleMes);
}
        private DetalleMesViewModel ObtenerDetalleMes(int año, int mes)
{
    var nombreMes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mes);
    var detalle = new DetalleMesViewModel
    {
        Año = año,
        Mes = mes,
        NombreMes = nombreMes
    };

    // Obtener SOLO las solicitudes REVISADAS del mes con todas las relaciones necesarias
    var solicitudesMes = _context.DbSetDescanso
        .Include(d => d.User)
        .Include(d => d.TipoDescanso)
        .Include(d => d.Accidente)
        .Include(d => d.Enfermedad)
        .Include(d => d.EnfermedadFam)
        .Include(d => d.Fallecimiento)
        .Include(d => d.Maternidad)
        .Include(d => d.Paternidad)
        .AsEnumerable()
        .Where(d => d.FechaSolicitud.Year == año &&
                   d.FechaSolicitud.Month == mes &&
                   // Solo incluir registros REVISADOS
                   ((d.EstadoSubsidioA == "Subsidio" || d.EstadoSubsidioA == "Rechazado") ||
                    (d.EstadoProcesado == "Aceptado" || d.EstadoProcesado == "Rechazado")))
        .ToList();

    var random = new Random();

    foreach (var solicitud in solicitudesMes)
    {
        // Determinar la categoría específica del descanso
        string tipoEspecifico = ObtenerTipoEspecifico(solicitud);

        // Determinar si es Subsidio o Descanso Médico para el estado
        string categoria = (solicitud.EstadoSubsidioA == "Subsidio" || solicitud.EstadoSubsidioA == "Rechazado")
            ? "Subsidio"
            : "Descanso Médico";

        // Generar ID único aleatorio (6 dígitos)
        var idUnico = random.Next(100000, 999999).ToString();

        detalle.Solicitudes.Add(new SolicitudDetalle
        {
            IdUnico = idUnico,
            Empleado = $"{solicitud.User.Username} {solicitud.User.Apellidos}",
            Dni = solicitud.User.Dni,
            Tipo = tipoEspecifico,
            Estado = categoria == "Subsidio" ? solicitud.EstadoSubsidioA : solicitud.EstadoProcesado,
            FechaSolicitud = solicitud.FechaSolicitud,
            IdDescanso = solicitud.IdDescanso
        });
    }

    // Ordenar por fecha de solicitud (más reciente primero)
    detalle.Solicitudes = detalle.Solicitudes
        .OrderByDescending(s => s.FechaSolicitud)
        .ToList();

    return detalle;
}
        private string ObtenerTipoEspecifico(Descanso descanso)
{
    // Verificar qué relación tiene datos para determinar el tipo específico
    if (descanso.Accidente != null)
        return "Accidente";
    else if (descanso.Maternidad != null)
        return "Maternidad";
    else if (descanso.Paternidad != null)
        return "Paternidad";
    else if (descanso.Enfermedad != null)
        return "Enfermedad";
    else if (descanso.EnfermedadFam != null)
        return "Enfermedad Familiar";
    else if (descanso.Fallecimiento != null)
        return "Fallecimiento";
    else
        return descanso.TipoDescanso?.Nombre ?? "No Especificado";
}


                

        public IActionResult MisSolicitudes()
        {
            // Obtener el ID del usuario logueado
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var misSolicitudes = _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.TipoDescanso)
                .Include(d => d.Accidente)
                .Include(d => d.Enfermedad)
                .Include(d => d.EnfermedadFam)
                .Include(d => d.Fallecimiento)
                .Include(d => d.Maternidad)
                .Include(d => d.Paternidad)
                .Where(d => d.User.IdUser == userId.Value)
                .AsEnumerable()
                .Select(d => new MisSolicitudesViewModel
                {
                    IdUnico = GenerarIdAleatorio(),
                    Tipo = DeterminarTipoSolicitud(d),
                    Motivo = ObtenerTipoEspecifico(d),
                    FechaEnvio = d.FechaSolicitud,
                    Estado = DeterminarEstadoGeneral(d),
                    ProcesoActual = DeterminarProcesoActual(d),
                    EstadoPSA = d.EstadoSubsidioA ?? "Pendiente",
                    EstadoESSALUD = d.EstadoESSALUD ?? "Pendiente",
                    EstadoAsistente = DeterminarEstadoAsistente(d),
                    EstadoJefe = d.EstadoSubsidioJ ?? "Pendiente",
                    IdDescanso = d.IdDescanso
                })
                .OrderByDescending(s => s.FechaEnvio)
                .ToList();

            ViewBag.TotalSolicitudes = misSolicitudes.Count;
            
            return View("MisSolicitudes", misSolicitudes);
        }

        // Método para determinar el estado del asistente
        private string DeterminarEstadoAsistente(Descanso descanso)
        {
            if (DeterminarTipoSolicitud(descanso) == "Descanso Médico")
            {
                if (descanso.EstadoESSALUD == "En Proceso")
                    return "Revisando";
                else if (descanso.EstadoESSALUD == "En Observación")
                    return "Observado";
                else if (!string.IsNullOrEmpty(descanso.EstadoProcesado))
                    return descanso.EstadoProcesado == "Aceptado" ? "Aprobado" : "Rechazado";
                else
                    return "Pendiente";
            }
            return "No aplica";
        }

        // Método actualizado para determinar estado general
        private string DeterminarEstadoGeneral(Descanso descanso)
        {
            if (DeterminarTipoSolicitud(descanso) == "Subsidio")
            {
                // Lógica para subsidios
                if (descanso.EstadoSubsidioA == "Rechazado")
                    return "Rechazado por PSA";
                else if (descanso.EstadoSubsidioA == "Subsidio" && descanso.EstadoSubsidioJ == "Aprobado")
                    return "Aprobado";
                else if (descanso.EstadoSubsidioA == "Subsidio" && descanso.EstadoSubsidioJ == "Rechazado")
                    return "Rechazado por Jefe";
                else if (descanso.EstadoSubsidioA == "Subsidio")
                    return "En Proceso (Jefe)";
                else
                    return "En Proceso (PSA)";
            }
            else
            {
                // Lógica para descansos médicos
                if (descanso.EstadoProcesado == "Aceptado")
                    return "Aprobado";
                else if (descanso.EstadoProcesado == "Rechazado")
                    return "Rechazado";
                else if (descanso.EstadoESSALUD == "En Observación")
                    return "En Observación";
                else
                    return "En Proceso";
            }
        }

        // Método actualizado para determinar proceso actual
        private string DeterminarProcesoActual(Descanso descanso)
        {
            if (DeterminarTipoSolicitud(descanso) == "Subsidio")
            {
                if (string.IsNullOrEmpty(descanso.EstadoSubsidioA) || descanso.EstadoSubsidioA == "Pendiente")
                    return "PSA";
                else if (descanso.EstadoSubsidioA == "Rechazado")
                    return "Finalizado (PSA)";
                else if (descanso.EstadoSubsidioA == "Subsidio" && (string.IsNullOrEmpty(descanso.EstadoSubsidioJ) || descanso.EstadoSubsidioJ == "Pendiente"))
                    return "Jefa de Bienestar";
                else if (descanso.EstadoSubsidioA == "Subsidio" && descanso.EstadoSubsidioJ == "Aprobado")
                    return "ESSALUD";
                else if (descanso.EstadoSubsidioA == "Subsidio" && descanso.EstadoSubsidioJ == "Rechazado")
                    return "Finalizado (Jefe)";
                else
                    return "Finalizado";
            }
            else
            {
                // Para descansos médicos
                if (descanso.EstadoESSALUD == "En Proceso")
                    return "Asistente de Bienestar";
                else if (descanso.EstadoESSALUD == "En Observación")
                    return "ESSALUD";
                else if (!string.IsNullOrEmpty(descanso.EstadoProcesado))
                    return "Jefa de Bienestar";
                else
                    return "Asistente de Bienestar";
            }
        }

        // Método auxiliar para generar ID aleatorio
        private string GenerarIdAleatorio()
        {
            var random = new Random();
            return $"REO_{random.Next(100, 999):000}";
        }

        // Método para determinar si es Subsidio o Descanso Médico
        private string DeterminarTipoSolicitud(Descanso descanso)
{
    // Calcular días del descanso
    var dias = (descanso.FechaFin - descanso.FechaIni).TotalDays;
    
    // Si es mayor a 30 días, es Subsidio, sino es Descanso Médico
    if (dias > 30)
    {
        return "Subsidio";
    }
    else
    {
        return "Descanso Médico";
    }
}

        // Método para determinar el estado general
        

        public IActionResult MiSeguimiento()
{
    // Obtener el ID del usuario logueado
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
    {
        return RedirectToAction("Login", "Auth");
    }

    var misSolicitudes = _context.DbSetDescanso
        .Include(d => d.User)
        .Include(d => d.TipoDescanso)
        .Include(d => d.Accidente)
        .Include(d => d.Enfermedad)
        .Include(d => d.EnfermedadFam)
        .Include(d => d.Fallecimiento)
        .Include(d => d.Maternidad)
        .Include(d => d.Paternidad)
        .Include(d => d.DocumentosMedicos)
        .Where(d => d.User.IdUser == userId.Value)
        .AsEnumerable()
        .Select(d => new MiSeguimientoViewModel
        {
            IdUnico = GenerarIdAleatorio(),
            Tipo = DeterminarTipoSolicitud(d),
            Motivo = ObtenerTipoEspecifico(d),
            FechaEnvio = d.FechaSolicitud,
            EstadoGeneral = DeterminarEstadoGeneral(d),
            Progreso = CalcularProgreso(d),
            FechaInicio = d.FechaIni,
            FechaFin = d.FechaFin,
            DiasTotales = (d.FechaFin - d.FechaIni).TotalDays,
            EstadoESSALUD = d.EstadoESSALUD ?? "Pendiente",
            EstadoSubsidioA = d.EstadoSubsidioA ?? "Pendiente",
            EstadoSubsidioJ = d.EstadoSubsidioJ ?? "Pendiente",
            EstadoProcesado = d.EstadoProcesado ?? "Pendiente",
            IdDescanso = d.IdDescanso,
            PasosCompletados = ObtenerPasosCompletados(d),
            TotalPasos = DeterminarTipoSolicitud(d) == "Descanso Médico" ? 4 : 4
        })
        .OrderByDescending(s => s.FechaEnvio)
        .ToList();

    ViewBag.TotalSolicitudes = misSolicitudes.Count;
    ViewBag.SolicitudesActivas = misSolicitudes.Count(s => s.EstadoGeneral == "En Proceso" || s.EstadoGeneral == "En Observación");
    ViewBag.SolicitudesFinalizadas = misSolicitudes.Count(s => s.EstadoGeneral == "Aprobado" || s.EstadoGeneral == "Rechazado");

    return View("MiSeguimiento", misSolicitudes);
}

// Método para calcular el progreso porcentual
private int CalcularProgreso(Descanso descanso)
{
    var tipoSolicitud = DeterminarTipoSolicitud(descanso);
    int pasosCompletados = ObtenerPasosCompletados(descanso);
    int totalPasos = tipoSolicitud == "Descanso Médico" ? 6 : 6; // Ambos tipos tienen 6 pasos

    return (int)Math.Round((double)pasosCompletados / totalPasos * 100);
}

// Método para obtener pasos completados - ACTUALIZADO
private int ObtenerPasosCompletados(Descanso descanso)
{
    var tipoSolicitud = DeterminarTipoSolicitud(descanso);
    int pasosCompletados = 1; // "Solicitud iniciada" siempre está completado

    if (tipoSolicitud == "Descanso Médico")
    {
        // Flujo para Descanso Médico (6 pasos)
        if (descanso.EstadoESSALUD == "Válido" || descanso.EstadoESSALUD == "No válido")
            pasosCompletados++; // "Aprobado por ESSALUD"

        if (!string.IsNullOrEmpty(descanso.EstadoProcesado))
            pasosCompletados++; // "Aprobado por el asistente de bienestar"

        if (descanso.EstadoSubsidioJ == "Aprobado")
            pasosCompletados++; // "Subsidio aprobado por la jefa de bienestar"

        if (descanso.EstadoSubsidioJ == "Aprobado")
            pasosCompletados++; // "Informado a planilla" (se completa automáticamente con la jefa)

        if (descanso.EstadoSubsidioJ == "Aprobado")
            pasosCompletados++; // "Subsidio" (se completa automáticamente con la jefa)
    }
    else
    {
        // Flujo para Subsidio (6 pasos)
        if (descanso.EstadoSubsidioA == "Subsidio")
            pasosCompletados++; // "Aprobado por PSA"

        if (descanso.EstadoSubsidioJ == "Aprobado")
            pasosCompletados++; // "Aprobado por la jefa de bienestar"

        if (descanso.EstadoSubsidioJ == "Aprobado")
            pasosCompletados++; // "Informado a planilla" (se completa automáticamente con la jefa)

        if (descanso.EstadoSubsidioJ == "Aprobado")
            pasosCompletados++; // "Subsidio" (se completa automáticamente con la jefa)

        // Para subsidios, ESSALUD también puede estar involucrado
        if (descanso.EstadoESSALUD == "Válido")
            pasosCompletados++; // "Aprobado por ESSALUD"
    }

    return pasosCompletados;
}

// Método actualizado para obtener el historial de procesos
private List<HistorialProceso> ObtenerHistorialProcesos(Descanso descanso)
{
    var historial = new List<HistorialProceso>();
    var tipoSolicitud = DeterminarTipoSolicitud(descanso);

    // Solicitud iniciada (siempre presente)
    historial.Add(new HistorialProceso
    {
        Etapa = "Solicitud iniciada",
        Fecha = descanso.FechaSolicitud,
        Completado = true
    });

    if (tipoSolicitud == "Descanso Médico")
    {
        // Flujo completo para Descanso Médico (6 pasos)
        historial.Add(new HistorialProceso
        {
            Etapa = "Aprobado por ESSALUD",
            Fecha = descanso.FechaSolicitud.AddDays(8),
            Completado = descanso.EstadoESSALUD == "Válido" || descanso.EstadoESSALUD == "No válido"
        });

        historial.Add(new HistorialProceso
        {
            Etapa = "Aprobado por el asistente de bienestar",
            Fecha = descanso.FechaSolicitud.AddDays(15),
            Completado = !string.IsNullOrEmpty(descanso.EstadoProcesado)
        });

        historial.Add(new HistorialProceso
        {
            Etapa = "Subsidio aprobado por la jefa de bienestar",
            Fecha = descanso.FechaSolicitud.AddDays(27),
            Completado = descanso.EstadoSubsidioJ == "Aprobado"
        });

        historial.Add(new HistorialProceso
        {
            Etapa = "Informado a planilla",
            Fecha = descanso.FechaSolicitud.AddDays(34),
            Completado = descanso.EstadoSubsidioJ == "Aprobado" // Se completa automáticamente
        });

        historial.Add(new HistorialProceso
        {
            Etapa = "Subsidio",
            Fecha = descanso.FechaSolicitud.AddDays(42),
            Completado = descanso.EstadoSubsidioJ == "Aprobado" // Se completa automáticamente
        });
    }
    else
    {
        // Flujo completo para Subsidio (6 pasos)
        historial.Add(new HistorialProceso
        {
            Etapa = "Aprobado por PSA",
            Fecha = descanso.FechaSolicitud.AddDays(5),
            Completado = descanso.EstadoSubsidioA == "Subsidio"
        });

        historial.Add(new HistorialProceso
        {
            Etapa = "Aprobado por ESSALUD",
            Fecha = descanso.FechaSolicitud.AddDays(12),
            Completado = descanso.EstadoESSALUD == "Válido"
        });

        historial.Add(new HistorialProceso
        {
            Etapa = "Aprobado por la jefa de bienestar",
            Fecha = descanso.FechaSolicitud.AddDays(19),
            Completado = descanso.EstadoSubsidioJ == "Aprobado"
        });

        historial.Add(new HistorialProceso
        {
            Etapa = "Informado a planilla",
            Fecha = descanso.FechaSolicitud.AddDays(26),
            Completado = descanso.EstadoSubsidioJ == "Aprobado" // Se completa automáticamente
        });

        historial.Add(new HistorialProceso
        {
            Etapa = "Subsidio",
            Fecha = descanso.FechaSolicitud.AddDays(33),
            Completado = descanso.EstadoSubsidioJ == "Aprobado" // Se completa automáticamente
        });
    }

    return historial;
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
