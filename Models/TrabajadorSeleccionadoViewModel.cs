using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    public class TrabajadorSeleccionadoViewModel
    {
    public int IdDescanso { get; set; }
    public string Dni { get; set; }
    public string Nombre { get; set; }
    public string Motivo { get; set; }
    public DateTime FechaIni { get; set; }
    public DateTime FechaFin { get; set; }
    public double Dias { get; set; }
    public string EstadoSubsidioA { get; set; }
    public string EstadoSubsidioJ { get; set; }
    }
}