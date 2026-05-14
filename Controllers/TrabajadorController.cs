using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using proyectoIngSoft.Data;
using proyectoIngSoft.ViewModels;

namespace proyectoIngSoft.Controllers
{    
    public class TrabajadoresController : Controller
    {
        private readonly ILogger<TrabajadoresController> _logger;
        private readonly ApplicationDbContext _context;

        public TrabajadoresController(ILogger<TrabajadoresController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }        

        // GET: /Trabajadores/Index
        public async Task<IActionResult> Index(string? filtro)
        {
            var query = _context.DbSetUser.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                filtro = filtro.Trim().ToLower();
                query = query.Where(u =>
                    u.Dni.ToLower().Contains(filtro) ||
                    u.Username.ToLower().Contains(filtro));
            }

            var trabajadores = await query.ToListAsync();

            // Agregar datos calculados (días acumulados y pago)
            var modelo = trabajadores.Select(u => new TrabajadorViewModel
            {
                Dni = u.Dni,
                Username = u.Username,
                Apellidos = u.Apellidos,
                CargoLaboral = u.CargoLaboral,
                DiasAcumulados = 30,
                TotalPagar = 10000
            }).ToList();

            return View(modelo);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}