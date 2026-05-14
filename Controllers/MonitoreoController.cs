using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyectoIngSoft.Data;
using proyectoIngSoft.Models;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Controllers
{
    public class MonitoreoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MonitoreoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Monitoreo?tipo=Subsidio&busqueda=
        public async Task<IActionResult> Index(string tipo, string busqueda)
        {
            // No bloqueamos el acceso, mostramos siempre el monitoreo
            IQueryable<Descanso> query = _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.DocumentosMedicos)
                .Include(d => d.TipoDescanso);

            // Filtrar por tipo (ejemplo: Subsidio)
            if (!string.IsNullOrEmpty(tipo) && tipo.ToLower() == "subsidio")
            {
                query = query.Where(d => d.EstadoSubsidioA == "Descanso Activo");
            }

            // Filtrar por búsqueda de trabajador
            if (!string.IsNullOrEmpty(busqueda))
            {
                busqueda = busqueda.ToLower();
                query = query.Where(d =>
                    d.User.Username.ToLower().Contains(busqueda) ||
                    d.User.Email.ToLower().Contains(busqueda) ||
                    d.User.Dni.ToLower().Contains(busqueda) ||
                    d.User.Apellidos.ToLower().Contains(busqueda));
            }

            var descansos = await query.OrderByDescending(d => d.FechaSolicitud).ToListAsync();
            return View(descansos);
        }

        // GET: /Monitoreo/Detalles/5
        public async Task<IActionResult> Detalles(int id)
        {
            var descanso = await _context.DbSetDescanso
                .Include(d => d.User)
                .Include(d => d.DocumentosMedicos)
                .Include(d => d.TipoDescanso)
                .FirstOrDefaultAsync(d => d.IdDescanso == id);

            if (descanso == null)
            {
                return NotFound();
            }

            return View(descanso);
        }

        // GET: /Monitoreo/VerDocumento/5
        public async Task<IActionResult> VerDocumento(int id)
        {
            var documento = await _context.DocumentosMedicos
                .Include(d => d.Descanso)
                .FirstOrDefaultAsync(d => d.IdDocumento == id);

            if (documento == null || documento.Archivo == null)
            {
                return NotFound();
            }

            // Retornar archivo PDF
            return File(documento.Archivo, "application/pdf", documento.Nombre);
        }
    }
}
