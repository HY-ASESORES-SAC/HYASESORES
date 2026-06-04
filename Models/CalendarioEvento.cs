using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    [Table("T_CalendarioEventos")]
    public class CalendarioEvento
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEvento { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        public string Titulo { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [Column(TypeName = "timestamp without time zone")]
        public DateTime FechaInicio { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? FechaFin { get; set; }

        [Required(ErrorMessage = "El tipo de evento es obligatorio")]
        public string TipoEvento { get; set; } = string.Empty; // "Cumpleaños", "Feriado", "EventoEmpresa", etc.

        public string Color { get; set; } = "#007bff";

        // Para eventos de cumpleaños
        public int? IdUser { get; set; }
        
        [ForeignKey("IdUser")]
        public User? User { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}