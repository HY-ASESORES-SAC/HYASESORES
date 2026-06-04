using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    [Table("t_Maternidad")]
    public class Maternidad
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdMater { get; set; }

        [Required]
        public DateOnly FechaParto { get; set; }
        [Required]
        public DateOnly FechaIni { get; set; }
        [Required]
        public DateOnly FechaFin { get; set; }
        [Required]
        public int SemanasGest { get; set; }
        [NotNull]
        [Required]
        public string PartoMult { get; set; }
        
        [Required]
        public DateOnly FechaUltM { get; set; }
        [NotNull]
        [Required]
        public string CentroMed { get; set; }
        [NotNull]
        [Required]
        public string MedicoT { get; set; }
        
        [Required]
        public string Descripcion { get; set; }


    }
}