using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    [Table("t_Enfermedad")]
    public class Enfermedad
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEnfermedad { get; set; }
       
        [NotNull]
        [Required]
        public string SubtipoSol { get; set; }
        [Required]
        public DateOnly FechaIni { get; set; }
        [Required]
        public DateOnly FechaFin { get; set; }
        [NotNull]
        [Required]
        public string NombreMedi { get; set; }
        [NotNull]
        [Required]
        public string CentroMedico { get; set; }
        [NotNull]
        [Required]
        public int DiasDesc { get; set; }
        [NotNull]
        [Required]
        public string Diagnostico { get; set; }
        [NotNull]
        [Required]
        public string DescEnfe { get; set; }
        
        


    }
}