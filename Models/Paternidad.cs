using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    [Table("t_Paternidad")]
    public class Paternidad
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPater { get; set; }
       

        [Required]
        public DateOnly FechaParto { get; set; }
        [Required]
        public DateOnly FechaIni { get; set; }
        [Required]
        public DateOnly FechaFin { get; set; }
        [NotNull]
        [Required]
        public string NombrePareja { get; set; }
        [NotNull]
        [Required]
        public string TipoSituacion { get; set; }
        
        [NotNull]
        [Required]
        public string CentroMed { get; set; }
        
        [Required]
        public DateOnly FechaComun { get; set; }


    }
}