using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    public class HistorialProceso
{
    public string Etapa { get; set; }
    public DateTime Fecha { get; set; }
    public bool Completado { get; set; }
}
}