using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    [Table("t_Fallecimiento")]
    public class Fallecimiento
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdFallec { get; set; }
      
        [Required]
        public string NombreFallec { get; set; }
        [Required]
        public DateOnly FechaIni { get; set; }
        [Required]
        public DateOnly FechaFin { get; set; }
        [NotNull]
        [Required]
        public string Parentesco { get; set; }
        [Required]
        public DateOnly FechaComun { get; set; }
        [NotNull]
        [Required]
        public string LugarSep { get; set; }
        [NotNull]
        [Required]
        public string Traslado { get; set; }
        
    }
}