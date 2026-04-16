    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace proyectoIngSoft.Models
    {
        public class Notification
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }              // PK
            public string UserId { get; set; }       // Usuario destino (puede ser el Id de Identity)
            public string Titulo { get; set; }
            public string Mensaje { get; set; }
            public string Estado { get; set; }       // En Observación, Aprobada, Rechazada
            public DateTime Fecha { get; set; }

            // Opcional - detalles extra
            public string Detalle { get; set; }
            public List<string> DocumentoAdjuntos { get; set; } = new List<string>();
        }
    }
