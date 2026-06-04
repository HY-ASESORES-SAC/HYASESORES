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


        // 🔥 Nueva propiedad: lista completa de empleados
        public List<EmpleadoViewModel> Empleados { get; set; } = new List<EmpleadoViewModel>();
    }

    // 🔹 Clase para los datos del empleado
    public class EmpleadoViewModel
    {
        public string Nombre { get; set; }
        public string Cargo { get; set; }
        public string Dni { get; set; }
        public string Email { get; set; }


    }
}
