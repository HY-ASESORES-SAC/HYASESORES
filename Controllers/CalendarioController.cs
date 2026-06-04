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
using proyectoIngSoft.Services;


namespace proyectoIngSoft.Controllers
{
    public class CalendarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalendarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Calendario
        public IActionResult Index(int? año, int? mes)
        {
            var fechaActual = DateTime.Now;
            var añoSeleccionado = año ?? fechaActual.Year;
            var mesSeleccionado = mes ?? fechaActual.Month;

            ViewBag.AñoActual = añoSeleccionado;
            ViewBag.MesActual = mesSeleccionado;
            ViewBag.MesNombre = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mesSeleccionado);
            ViewBag.FechaActual = fechaActual;

            // Obtener eventos del mes - SOLO eventos que ocurren en días específicos
            var primerDiaMes = new DateTime(añoSeleccionado, mesSeleccionado, 1);
            var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

            var eventos = _context.DbSetCalendarioEvento
                .Where(e => e.FechaInicio.Date >= primerDiaMes.Date &&
                           e.FechaInicio.Date <= ultimoDiaMes.Date)
                .Include(e => e.User)
                .ToList();

            ViewBag.Eventos = eventos;

            return View();
        }

        // GET: /Calendario/GetEventosDia
        public JsonResult GetEventosDia(int año, int mes, int dia)
        {
            try
            {
                var fecha = new DateTime(año, mes, dia);

                Console.WriteLine($"Buscando eventos para: {fecha:yyyy-MM-dd}"); // DEBUG

                var eventos = _context.DbSetCalendarioEvento
                    .Where(e => e.FechaInicio.Date == fecha.Date)
                    .Include(e => e.User)
                    .Select(e => new
                    {
                        e.IdEvento,
                        e.Titulo,
                        e.Descripcion,
                        e.TipoEvento,
                        e.Color,
                        Empleado = e.User != null ? $"{e.User.Username} {e.User.Apellidos}" : null,
                        Dni = e.User != null ? e.User.Dni : null
                    })
                    .ToList();

                Console.WriteLine($"Se encontraron {eventos.Count} eventos"); // DEBUG
                foreach (var evento in eventos)
                {
                    Console.WriteLine($"Evento: ID={evento.IdEvento}, Titulo='{evento.Titulo}', Descripcion='{evento.Descripcion}'"); // DEBUG
                }

                return Json(eventos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetEventosDia: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new List<object>());
            }
        }

        // GET: /Calendario/BuscarEmpleados
        public JsonResult BuscarEmpleados(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return Json(new List<object>());

            var empleados = _context.DbSetUser
                .Where(u => u.Username != null && u.Username.Contains(termino) ||
                           u.Apellidos != null && u.Apellidos.Contains(termino) ||
                           u.Dni != null && u.Dni.Contains(termino) ||
                           u.CargoLaboral != null && u.CargoLaboral.Contains(termino))
                .Take(10)
                .Select(u => new
                {
                    id = u.IdUser,
                    text = $"{u.Username} {u.Apellidos} - {u.Dni} - {u.CargoLaboral}",
                    nombre = u.Username,
                    apellidos = u.Apellidos,
                    dni = u.Dni,
                    cargo = u.CargoLaboral,
                    fechaNacimiento = u.FechaNacimiento.ToString("yyyy-MM-dd")
                })
                .ToList();

            return Json(empleados);
        }

        // POST: /Calendario/AgregarEvento
        [HttpPost]
        public async Task<IActionResult> AgregarEvento([FromBody] CalendarioEvento evento)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Console.WriteLine($"Datos recibidos para evento:");
                    Console.WriteLine($"Titulo: '{evento.Titulo}'");
                    Console.WriteLine($"Descripcion: '{evento.Descripcion}'");
                    Console.WriteLine($"TipoEvento: '{evento.TipoEvento}'");
                    Console.WriteLine($"FechaInicio: '{evento.FechaInicio}'");
                    Console.WriteLine($"IdUser: '{evento.IdUser}'");

                    // Validar que el título no esté vacío
                    if (string.IsNullOrWhiteSpace(evento.Titulo))
                    {
                        return Json(new { success = false, message = "El título es obligatorio" });
                    }

                    // Asegurar que las fechas estén en formato correcto para PostgreSQL
                    evento.FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                    evento.FechaInicio = DateTime.SpecifyKind(evento.FechaInicio.Date, DateTimeKind.Unspecified); // Solo la fecha
                    evento.FechaFin = null; // Los cumpleaños son de un solo día

                    // Para cumpleaños, usar color rojo
                    if (evento.TipoEvento == "Cumpleaños")
                    {
                        evento.Color = "#FF6B6B";
                    }

                    _context.DbSetCalendarioEvento.Add(evento);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"✅ Evento guardado correctamente - ID: {evento.IdEvento}, Titulo: '{evento.Titulo}'");

                    return Json(new { success = true, message = "Evento agregado correctamente", id = evento.IdEvento });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error al guardar evento: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    return Json(new { success = false, message = $"Error al guardar: {ex.Message}" });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            Console.WriteLine($"❌ Errores de validación: {string.Join(", ", errors)}");
            return Json(new { success = false, message = "Datos del evento no válidos" });
        }
        // POST: /Calendario/AgregarEventoFestivo
        [HttpPost]
        public async Task<IActionResult> AgregarEventoFestivo([FromBody] CalendarioEvento evento)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Console.WriteLine($"Datos recibidos - Titulo: '{evento.Titulo}', Descripcion: '{evento.Descripcion}'");

                    // Validar que el título no esté vacío
                    if (string.IsNullOrWhiteSpace(evento.Titulo))
                    {
                        return Json(new { success = false, message = "El título es obligatorio" });
                    }

                    // Configurar como evento festivo
                    evento.TipoEvento = "Feriado";
                    evento.Color = "#4A90E2";
                    evento.FechaCreacion = DateTime.Now;
                    evento.IdUser = null;
                    evento.FechaFin = null;

                    // Asegurar que solo sea la fecha
                    evento.FechaInicio = evento.FechaInicio.Date;

                    _context.DbSetCalendarioEvento.Add(evento);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"Evento guardado - ID: {evento.IdEvento}, Titulo: '{evento.Titulo}'");

                    return Json(new { success = true, message = "Evento festivo agregado correctamente", id = evento.IdEvento });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error guardando evento: {ex.Message}");
                    return Json(new { success = false, message = $"Error al guardar: {ex.Message}" });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            Console.WriteLine($"Errores de validación: {string.Join(", ", errors)}");
            return Json(new { success = false, message = "Datos del evento no válidos" });
        }


        [HttpPost]
        public async Task<IActionResult> ProcesarNotificacionesCumpleanos()
        {
            try
            {
                var service = new NotificacionCumpleanosService(_context, 
                    LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<NotificacionCumpleanosService>());
                
                await service.EnviarNotificacionesCumpleanos();
                return Json(new { success = true, message = "Notificaciones procesadas" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }






    }
}