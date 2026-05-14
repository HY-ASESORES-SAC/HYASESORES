using System;

namespace proyectoIngSoft.Models
{
    public class NotificacionSimulada
    {
        public int Id { get; set; }          // 🔑 Clave primaria obligatoria
        public string De { get; set; }       // Quién envía la notificación
        public string Para { get; set; }     // Quién recibe
        public string Tipo { get; set; }     // Inactividad / Reincorporación
        public string Mensaje { get; set; }  // Contenido del mensaje
        public DateTime Fecha { get; set; }  // Fecha de envío
    }
}
