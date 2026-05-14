using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace proyectoIngSoft.Models
{
    [Table("T_Usuarios")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUser { get; set; }

        [NotNull]
        [Required, MaxLength(100)]
        public string Username { get; set; }

        [NotNull]
        [Required, MaxLength(100)]
        public string Email { get; set; }

        [NotNull]
        [Required]
        public string Password { get; set; }

        [NotNull]
        [Required]
        public string Apellidos { get; set; }

        [NotNull]
        [Required]
        public string Dni { get; set; }

        [NotNull]
        [Required, DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }

        [NotNull]
        [Required]
        public string Telefono { get; set; }

        [NotNull]
        [Required]
        public string Ubigeo { get; set; }

        [NotNull]
        [Required]
        public string Distrito { get; set; }

        [Required, MaxLength(6)]
        public string RazonSocial { get; set; }

        [ForeignKey("CodigoSocial")]
        public int? IdCodigo { get; set; }

        public CodigoSocial? CodigoSocial { get; set; }

        public string? CargoLaboral { get; set; } // opcional

        [NotNull]
        [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarPassword { get; set; }

        public string Rol { get; set; } = "Trabajador";

        // 🔹 Propiedad calculada para mostrar el nombre completo
        [NotMapped]
        public string NombreCompleto => $"{Username} {Apellidos}";
    }
}
