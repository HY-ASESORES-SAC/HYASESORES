using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    [Table("t_EnfermedadFamiliar")]
    public class EnfermedadFam
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEnfermedadFam { get; set; }
    
        [Required]
        public string NombreFamiliar { get; set; }
        [Required]
        public DateOnly FechaIni { get; set; }
        [Required]
        public DateOnly FechaFin { get; set; }
        [NotNull]
        [Required]
        public string Parentesco { get; set; }
        [NotNull]
        [Required]
        public string CentroMedico { get; set; }
        [NotNull]
        [Required]
        public string Medico { get; set; }
        [NotNull]
        [Required]
        public string NumeroCMP { get; set; }
        [Required]
        public DateOnly FechaDiag { get; set; }
        [Required]
        public int DiaSoli { get; set; }


    }
}