using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
   public class MiSeguimientoViewModel
    {
        public string IdUnico { get; set; }
        public string Tipo { get; set; } // "Subsidio" o "Descanso Médico"
        public string Motivo { get; set; }
        public DateTime FechaEnvio { get; set; }
        public string EstadoGeneral { get; set; }
        public int Progreso { get; set; } // Porcentaje de progreso
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public double DiasTotales { get; set; }
        public string EstadoESSALUD { get; set; }
        public string EstadoSubsidioA { get; set; }
        public string EstadoSubsidioJ { get; set; }
        public string EstadoProcesado { get; set; }
        public int IdDescanso { get; set; }
        public int PasosCompletados { get; set; }
        public int TotalPasos { get; set; }
    }
}