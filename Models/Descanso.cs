using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace proyectoIngSoft.Models
{
    [Table("t_Descanso")]
    public class Descanso
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdDescanso { get; set; }

        // Relación con User
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        // Relación con TipoDescanso
        [Required]
        public int TipoDescansoId { get; set; }
        public TipoDescanso TipoDescanso { get; set; }


        [Required]
        public DateTime FechaIni { get; set; }

        [Required]
        public DateTime FechaFin { get; set; } 

        [Required]
        public DateTime FechaSolicitud { get; set; }


        // FKs opcionales según el tipo
        public int? AccidenteId { get; set; }
        public Accidente? Accidente { get; set; }

        public int? MaternidadId { get; set; }
        public Maternidad? Maternidad { get; set; }

        public int? PaternidadId { get; set; }
        public Paternidad? Paternidad { get; set; }

        public int? EnfermedadId { get; set; }
        public Enfermedad? Enfermedad { get; set; }
        public int? FallecimientoId { get; set; }
        public Fallecimiento? Fallecimiento { get; set; }

        public int? EnfermedadFamId { get; set; }
        public EnfermedadFam? EnfermedadFam { get; set; }
        public ICollection<DocumentoMedico> DocumentosMedicos { get; set; } = new List<DocumentoMedico>();


        [StringLength(50)]
        public string EstadoESSALUD { get; set; } = "En Proceso"; // En Proceso | Válido | No válido

        [StringLength(50)]
        public string EstadoSubsidioA { get; set; } = "Descanso Activo"; 

        [StringLength(50)]
        public string EstadoSubsidioJ { get; set; } = "Pendiente"; 
    }
}