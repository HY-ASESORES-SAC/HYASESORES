using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    public class DetalleSolicitudCompletoViewModel
    {
        public Descanso Descanso { get; set; }
        public List<HistorialProceso> HistorialProcesos { get; set; }
    }
}