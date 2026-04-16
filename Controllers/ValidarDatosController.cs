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
   
    public class ValidarDatosController : Controller
    {
        private readonly ILogger<ValidarDatosController> _logger;
        private readonly ApplicationDbContext _context;

        public ValidarDatosController(ILogger<ValidarDatosController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Vista inicial
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Captcha = GenerarCaptcha();
            return View(new ValidarDatos());
        }

        // POST: Procesar datos
        [HttpPost]
        public IActionResult Index(ValidarDatos model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Captcha = GenerarCaptcha();
                return View(model);
            }

            if (model.Captcha != TempData["Captcha"]?.ToString())
            {
                ModelState.AddModelError("Captcha", "El código ingresado no es correcto");
                ViewBag.Captcha = GenerarCaptcha();
                return View(model);
            }

            // 🔹 Verificar si el DNI existe en la tabla de usuarios
            var usuario = _context.DbSetUser.FirstOrDefault(u => u.Dni == model.DNI);

            if (usuario == null)
            {
                ModelState.AddModelError("DNI", "El DNI ingresado no está registrado en el sistema.");
                ViewBag.Captcha = GenerarCaptcha();
                return View(model);
            }

            try
            {
                // Guardamos la validación solo si el usuario existe
                _context.ValidarDatos.Add(model);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar los datos de validación");
                ModelState.AddModelError("", "Ocurrió un error al guardar los datos.");
                return View(model);
            }

            // 🔹 Redirigir a confirmación si todo ok
            return RedirectToAction("Index", "ConfirmacionIdentidad");

        }

        // Acción para mostrar imagen del DNI con ubicación del UBIGEO
        public IActionResult AyudaUbigeo()
        {
            return View();
        }

        private string GenerarCaptcha()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            TempData["Captcha"] = result;
            return result;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}