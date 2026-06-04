using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    public class DetalleMesViewModel
    {
        public int Año { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; }
        public List<SolicitudDetalle> Solicitudes { get; set; } = new List<SolicitudDetalle>();
    }
}