using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    public class SolicitudDetalle
    {
        public string IdUnico { get; set; } // ID generado aleatorio
        public string Empleado { get; set; }
        public string Dni { get; set; }
        public string Tipo { get; set; } // "Subsidio" o "Descanso Médico"
        public string Estado { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int IdDescanso { get; set; } // ID real de la base de datos
    }
}