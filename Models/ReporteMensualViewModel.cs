using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    public class ReporteMensualViewModel
    {
        public int Año { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; }
        public int TotalRevisados { get; set; }
        public int Subsidios { get; set; }
        public int DescansosMedicos { get; set; }
        public int Aprobados { get; set; }
        public int Rechazados { get; set; }
    }
}