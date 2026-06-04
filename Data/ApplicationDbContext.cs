using Microsoft.EntityFrameworkCore;
using proyectoIngSoft.Models;

namespace proyectoIngSoft.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Accidente> DbSetAccidente { get; set; }
        public DbSet<Maternidad> DbSetMaternidad { get; set; }
        public DbSet<Paternidad> DbSetPaternidad { get; set; }
        public DbSet<Enfermedad> DbSetEnfermedad { get; set; }
        public DbSet<Fallecimiento> DbSetFallecimiento { get; set; }
        public DbSet<DocumentoMedico> DocumentosMedicos { get; set; }
        public DbSet<EnfermedadFam> DbSetEnfermedadF { get; set; }
        public DbSet<User> DbSetUser { get; set; }
        public DbSet<Descanso> DbSetDescanso { get; set; }
        public DbSet<TipoDescanso> DbSetTipoDescanso { get; set; }
        public DbSet<ValidarDatos> ValidarDatos { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<CodigoSocial> DbSetCodigoSocial { get; set; }
        public DbSet<NotificacionSimulada> DbSetNotificacionSimulada { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración para PostgreSQL
            modelBuilder.Entity<Notification>()
                .Property(n => n.DocumentoAdjuntos)
                .HasColumnType("text[]");

            modelBuilder.Entity<User>()
                .Property(u => u.FechaNacimiento)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<TipoDescanso>().HasData(
                new TipoDescanso { IdTDescanso = 1, Nombre = "Enfermedad" },
                new TipoDescanso { IdTDescanso = 2, Nombre = "Maternidad" },
                new TipoDescanso { IdTDescanso = 3, Nombre = "Paternidad" },
                new TipoDescanso { IdTDescanso = 4, Nombre = "Fallecimiento Familiar" },
                new TipoDescanso { IdTDescanso = 5, Nombre = "Enfermedad Familiar" },
                new TipoDescanso { IdTDescanso = 6, Nombre = "Accidente" }
            );
        }
    }
}
