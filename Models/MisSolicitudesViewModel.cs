using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    namespace proyectoIngSoft.Models
    {
        public class MisSolicitudesViewModel
        {
            public string IdUnico { get; set; }
            public string Tipo { get; set; } // "Descanso Médico" o "Subsidio"
            public string Motivo { get; set; }
            public DateTime FechaEnvio { get; set; }
            public string Estado { get; set; } // Aprobado, Rechazado, En Proceso, En Observación
            public string ProcesoActual { get; set; }
            public string EstadoPSA { get; set; } // Nuevo: Estado específico de PSA
            public string EstadoESSALUD { get; set; } // Nuevo: Estado específico de ESSALUD
            public string EstadoAsistente { get; set; } // Nuevo: Estado específico del Asistente
            public string EstadoJefe { get; set; } // Nuevo: Estado específico del Jefe
            public int IdDescanso { get; set; }
        }
    }
}