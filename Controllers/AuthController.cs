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

    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ApplicationDbContext _context;

        public AuthController(ILogger<AuthController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }


        // GET: /Auth/Register
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                // 🔹 Normalizar y validar longitud del código (exactamente 6 caracteres)
                var codigoIngresado = (user.RazonSocial ?? string.Empty).Trim().ToUpper();

                if (codigoIngresado.Length != 6)
                {
                    ModelState.AddModelError("RazonSocial", "El código debe tener exactamente 6 caracteres.");
                    return View(user);
                }

                // 🔹 Buscar el código ingresado en la tabla CodigoSocial
                var codigoSocial = _context.DbSetCodigoSocial
                    .FirstOrDefault(c => c.Codigo.ToUpper() == codigoIngresado);

                if (codigoSocial == null)
                {
                    ModelState.AddModelError("RazonSocial", "❌ El código ingresado no es válido.");
                    return View(user);
                }

                // 🔹 Asignar el rol y la relación foránea
                user.Rol = codigoSocial.Rol;
                user.IdCodigo = codigoSocial.IdCodigo; // FK hacia la tabla CodigoSocial

                // 🔹 Guardar usuario
                _context.DbSetUser.Add(user);
                _context.SaveChanges();

                // 🔹 Guardamos en TempData para mostrar modal de éxito
                TempData["RegistroExitoso"] = "✅ Registro completado correctamente.";

                // 🔹 Redirigimos a la misma vista (Register) para mostrar el modal
                return RedirectToAction("Register");
            }

            // Si hay errores de validación
            return View(user);
        }

        // GET: /Auth/Login
        public IActionResult Login() => View();

        [HttpPost]
     [HttpPost]
public IActionResult Login(string email, string password)
{
    var user = _context.DbSetUser.FirstOrDefault(u => u.Email == email && u.Password == password);
    if (user != null)
    {
        // Guardar sesión usando Email
        HttpContext.Session.SetString("User", user.Email);
        HttpContext.Session.SetString("Rol", user.Rol);
        return RedirectToAction("Index", "Home");
    }

    ViewBag.Error = "Correo o contraseña incorrectos";
    return View();
}


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}