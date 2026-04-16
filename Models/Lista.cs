using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoIngSoft.Models
{
    public class Lista
    {
        public string Username { get; set; }
        public int IdDescanso { get; set; } 
        public string Apellidos { get; set; }
        public string Dni { get; set; }
        public string Observaciones { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string Estado { get; set; }
        public int IdUser { get; set; }
    }
}