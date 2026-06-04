using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
public async Task<IActionResult> Login(string email, string password)
{
    var user = _context.DbSetUser.FirstOrDefault(u => u.Email == email && u.Password == password);
    if (user != null)
    {
        // 🔹 Crear Claims para el usuario
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Surname, user.Apellidos),
            new Claim(ClaimTypes.Role, user.Rol),
            new Claim("Dni", user.Dni)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        // 🔹 Autenticar con cookies
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        // 🔹 Guardar también en sesión para compatibilidad con código existente
        HttpContext.Session.SetString("User", user.Email);
        HttpContext.Session.SetString("Rol", user.Rol);
        HttpContext.Session.SetInt32("UserId", user.IdUser);

        return RedirectToAction("Index", "Home");
    }

    ViewBag.Error = "Correo o contraseña incorrectos";
    return View();
}


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}