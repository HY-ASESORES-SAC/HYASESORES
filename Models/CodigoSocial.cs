using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    public class CodigoSocial
    {
        [Key]
        public int IdCodigo { get; set; }

        [Required, MaxLength(6)]
        public string Codigo { get; set; }

        [Required]
        public string Rol { get; set; }
        
    }
}