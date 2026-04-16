using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    [Table("t_Accidente")]
    public class Accidente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdAccidente { get; set; }
        
        [Required]
        public string NombreComp { get; set; }
        [NotNull]
        [Required]
        public int DNI { get; set; }
        
        [Required]
        public DateOnly FechaIni { get; set; }
        [Required]
        public DateOnly FechaFin { get; set; }
       
        [NotNull]
        
        public string Observaciones { get; set; }

        [NotNull]
        [Required]
        public string TipoDM { get; set; }


    }
}