using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace proyectoIngSoft.Models
{
    public class SimulacionNotificacionViewModel
    {
        [Display(Name = "Buscar Trabajador")]
        public string Busqueda { get; set; }

        [Display(Name = "Trabajadores encontrados")]
        public List<string> Trabajadores { get; set; } = new List<string>();

        [Display(Name = "Cargos seleccionados")]
        public List<string> CargosSeleccionados { get; set; } = new List<string>();

        [Display(Name = "Cargos disponibles")]
        public List<string> CargosDisponibles { get; set; } = new List<string>();

        [Display(Name = "Tipo de notificación")]
        public string TipoNotificacion { get; set; }

        [Display(Name = "Mensaje de notificación")]
        public string Mensaje { get; set; }
    }
}
