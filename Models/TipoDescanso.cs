using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    [Table("t_TiposDescanso")]
    public class TipoDescanso
    {
        [Key]
        public int IdTDescanso { get; set; }
        [Required]
        public string Nombre { get; set; } // Ejemplo: "Accidente", "Maternidad"
        public ICollection<Descanso> Descansos { get; set; }
    }
}