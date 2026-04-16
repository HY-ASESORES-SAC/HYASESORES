using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    public class ValidarDatos
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [StringLength(8, ErrorMessage = "El DNI no puede tener más de 8 dígitos")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "El DNI solo debe contener números")]
        public string DNI { get; set; }

        [Required(ErrorMessage = "El UBIGEO es obligatorio")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "El UBIGEO debe tener exactamente 6 dígitos")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "El UBIGEO solo debe contener números")]
        public string Ubigeo { get; set; }

        [Required(ErrorMessage = "Debe ingresar el código CAPTCHA")]
        public string Captcha { get; set; }
    }
}