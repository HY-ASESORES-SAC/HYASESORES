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
   
    public class SistemaSolicitudController : Controller
    {
        private readonly ILogger<SistemaSolicitudController> _logger;
        
        private readonly ApplicationDbContext _context;

        public SistemaSolicitudController(ILogger<SistemaSolicitudController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        

        public IActionResult Index()
        {

            return View();
        }

        public IActionResult ListaSolicitudes()
        {
            // var lista = _context.DbSetDescanso
            // .Include(d => d.User)
            // .Include(d => d.TipoDescanso)
            // .Select(d => new Lista
            // {
            //     Username = d.User.Username,
            //     Apellidos = d.User.Apellidos,
            //     Dni = d.User.Dni,
            //     Observaciones = d.TipoDescanso.Nombre,
            //     FechaSolicitud = d.FechaSolicitud,
            //     Estado = d.EstadoESSALUD ?? "En Proceso",
            //     IdUser = d.User.IdUser,
            //     IdDescanso = d.IdDescanso
            // })
            // .ToList();

            // return View("ListaSolicitudes", lista);

            var descansos = _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.TipoDescanso)
                .ToList();

            var pendientes = descansos
                .Where(d => d.EstadoESSALUD == null || d.EstadoESSALUD == "En Proceso")
                .Select(d => new Lista
                {
                    IdDescanso = d.IdDescanso,
                    Username = d.User.Username,
                    Apellidos = d.User.Apellidos,
                    Dni = d.User.Dni,
                    Observaciones = d.TipoDescanso.Nombre,
                    FechaSolicitud = d.FechaSolicitud,
                    Estado = d.EstadoESSALUD ?? "En Proceso",
                    IdUser = d.User.IdUser
                })
                .ToList();

            var procesadas = descansos
                .Where(d => d.EstadoESSALUD == "Válido" || d.EstadoESSALUD == "No válido")
                .Select(d => new Lista
                {
                    IdDescanso = d.IdDescanso,
                    Username = d.User.Username,
                    Apellidos = d.User.Apellidos,
                    Dni = d.User.Dni,
                    Observaciones = d.TipoDescanso.Nombre,
                    FechaSolicitud = d.FechaSolicitud,
                    Estado = d.EstadoESSALUD,
                    IdUser = d.User.IdUser
                })
                .ToList();

            ViewBag.Pendientes = pendientes;
            ViewBag.Procesadas = procesadas;

            return View();

        }



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

        public IActionResult VerDocumento(int id)
        {
            var documento = _context.DocumentosMedicos
                .AsNoTracking()
                .FirstOrDefault(d => d.IdDocumento == id);

            if (documento == null || documento.Archivo == null || documento.Archivo.Length == 0)
                return NotFound();

            // Forzar inline para que se vea en el iframe
            Response.Headers["Content-Disposition"] = $"inline; filename=\"{documento.Nombre}\"";
            return File(documento.Archivo, "application/pdf");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarEstadosESSALUD([FromBody] List<EstadoUpdateDto> solicitudes)
        {
            if (solicitudes == null || !solicitudes.Any())
                return BadRequest("No hay solicitudes para actualizar.");

            foreach (var item in solicitudes)
            {
                var descanso = await _context.DbSetDescanso.FindAsync(item.IdDescanso);
                if (descanso != null)
                {
                    descanso.EstadoESSALUD = item.Estado;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Estados actualizados correctamente." });
        }

        public class EstadoUpdateDto
        {
            public int IdDescanso { get; set; }
            public string Estado { get; set; } = "";
        }
    }
}