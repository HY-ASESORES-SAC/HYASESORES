using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using proyectoIngSoft.Data;
using proyectoIngSoft.Models;

namespace proyectoIngSoft.Controllers
{
   
    public class EnfermedadFamController : Controller
    {
        private readonly ILogger<EnfermedadFamController> _logger;
        private readonly ApplicationDbContext _context;

        public EnfermedadFamController(ILogger<EnfermedadFamController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var documentos = _context.DocumentosMedicos.ToList();
            return View();
        }
        [HttpPost]
       
        public IActionResult Registrar(EnfermedadFam model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Guardar Accidente
                    _context.DbSetEnfermedadF.Add(model);
                    _context.SaveChanges();

                    // 2. Obtener usuario logueado (simulado)
                    var username = HttpContext.Session.GetString("User");
                    if (string.IsNullOrEmpty(username))
                    {
                        ViewData["Message"] = "No hay usuario logueado";
                        return View("Index", model);
                    }

                    var user = _context.DbSetUser.FirstOrDefault(u => u.Username == username);
                    if (user == null)
                    {
                        ViewData["Message"] = "Usuario no encontrado";
                        return View("Index", model);
                    }

                    // 3. Crear Descanso
                    var descanso = new Descanso
                    {
                        UserId = user.IdUser,               // FK a T_Usuarios
                        TipoDescansoId = 5,                 // 1 = Accidente
                        FechaSolicitud = DateTime.UtcNow,
                        EnfermedadFamId = model.IdEnfermedadFam     // FK al Accidente recién creado
                    };

                    _context.DbSetDescanso.Add(descanso);
                    _context.SaveChanges();

                    ViewData["Message"] = "Accidente registrado con éxito";
                    return RedirectToAction("Index", "DocumentoMedico", new { descansoId = descanso.IdDescanso });
            }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al registrar el descanso.");
                    ViewData["Message"] = "Error al registrar el descanso: " + ex.Message;
                }
            }
            else
            {
                ViewData["Message"] = "Datos de entrada no válidos";
            }
            return View("Index");
            

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}