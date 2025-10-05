using Microsoft.EntityFrameworkCore;
using YSA.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Linq;

namespace YSA.Data.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario, Rol, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Artista> Artistas { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<Leccion> Lecciones { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<ProductoCategoria> ProductoCategorias { get; set; }
        public DbSet<CursoCategoria> CursoCategorias { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoItem> PedidoItems { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<EstudianteCurso> EstudianteCursos { get; set; }
        public DbSet<VentaItem> VentaItems { get; set; }
        public DbSet<PreguntaRespuesta> PreguntasRespuestas { get; set; }
        public DbSet<Anuncio> Anuncios { get; set; }
        public DbSet<Resena> Resenas { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<ProgresoLeccion> ProgresoLecciones { get; set; }
        public DbSet<ArtistaFoto> ArtistaFotos { get; set; }
        public DbSet<Evento> Eventos { get; set; }
        public DbSet<EventoFotos> EventoFotos { get; set; }
        public DbSet<TipoEvento> TipoEventos { get; set; }
        public DbSet<RecursoActividad> RecursosActividades { get; set; }
        public DbSet<EntregaActividad> EntregasActividades { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            modelBuilder.Entity<ProductoCategoria>()
                .HasKey(pc => new { pc.ProductoId, pc.CategoriaId });

            modelBuilder.Entity<ProductoCategoria>()
                .HasOne(pc => pc.Producto)
                .WithMany(p => p.ProductoCategorias)
                .HasForeignKey(pc => pc.ProductoId);

            modelBuilder.Entity<ProductoCategoria>()
                .HasOne(pc => pc.Categoria)
                .WithMany(c => c.ProductoCategorias)
                .HasForeignKey(pc => pc.CategoriaId);

            modelBuilder.Entity<CursoCategoria>()
                .HasKey(cc => new { cc.CursoId, cc.CategoriaId });

            modelBuilder.Entity<CursoCategoria>()
                .HasOne(cc => cc.Curso)
                .WithMany(c => c.CursoCategorias)
                .HasForeignKey(cc => cc.CursoId);

            modelBuilder.Entity<CursoCategoria>()
                .HasOne(cc => cc.Categoria)
                .WithMany(c => c.CursoCategorias)
                .HasForeignKey(cc => cc.CategoriaId);

            modelBuilder.Entity<Curso>()
                .HasOne(c => c.Instructor)
                .WithMany(a => a.Cursos)
                .HasForeignKey(c => c.InstructorId);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Autor)
                .WithMany(a => a.Productos)
                .HasForeignKey(p => p.AutorId);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Estudiante)
                .WithMany(e => e.Pedidos)
                .HasForeignKey(p => p.EstudianteId);

            modelBuilder.Entity<PedidoItem>()
                .HasOne(pi => pi.Pedido)
                .WithMany(p => p.PedidoItems)
                .HasForeignKey(pi => pi.PedidoId);

            // Se ha cambiado la relación para que PedidoItem apunte a VentaItem
            modelBuilder.Entity<PedidoItem>()
                .HasOne(pi => pi.VentaItem)
                .WithMany()
                .HasForeignKey(pi => pi.VentaItemId);

            modelBuilder.Entity<Pago>()
                .HasOne(pa => pa.Pedido)
                .WithOne(pe => pe.Pago)
                .HasForeignKey<Pago>(pa => pa.PedidoId);

            modelBuilder.Entity<Pago>()
                .HasOne(pa => pa.Validador)
                .WithMany()
                .HasForeignKey(pa => pa.ValidadorId);

            // Nuevas configuraciones para la entidad VentaItem
            modelBuilder.Entity<VentaItem>()
                .HasOne(vi => vi.Curso)
                .WithMany()
                .HasForeignKey(vi => vi.CursoId);

            modelBuilder.Entity<VentaItem>()
                .HasOne(vi => vi.Producto)
                .WithMany()
                .HasForeignKey(vi => vi.ProductoId);

            // Configuración para la clave compuesta de Resena
            modelBuilder.Entity<Resena>()
                .HasKey(r => new { r.EstudianteId, r.CursoId });
            
            // Nueva configuración para PreguntaRespuesta
            modelBuilder.Entity<PreguntaRespuesta>()
                .HasOne(pr => pr.Curso)
                .WithMany(c => c.PreguntasRespuestas)
                .HasForeignKey(pr => pr.CursoId);
            
            modelBuilder.Entity<PreguntaRespuesta>()
                .HasOne(pr => pr.Estudiante)
                .WithMany()
                .HasForeignKey(pr => pr.EstudianteId);

            modelBuilder.Entity<PreguntaRespuesta>()
                .HasOne(pr => pr.Instructor)
                .WithMany(i => i.PreguntasRespuestas)
                .HasForeignKey(pr => pr.InstructorId);
            
            // Nueva configuración para Anuncio
            modelBuilder.Entity<Anuncio>()
                .HasOne(a => a.Curso)
                .WithMany(c => c.Anuncios)
                .HasForeignKey(a => a.CursoId);

            // Nueva configuración para Resena
            modelBuilder.Entity<Resena>()
                .HasOne(r => r.Estudiante)
                .WithMany()
                .HasForeignKey(r => r.EstudianteId);

            modelBuilder.Entity<Resena>()
                .HasOne(r => r.Curso)
                .WithMany(c => c.Resenas)
                .HasForeignKey(r => r.CursoId);

            // Nueva configuración para MetodoPago
            modelBuilder.Entity<MetodoPago>()
                .HasOne(mp => mp.Estudiante)
                .WithMany()
                .HasForeignKey(mp => mp.EstudianteId);
            modelBuilder.Entity<VentaItem>()
                .HasMany(vi => vi.PedidoItems) 
                .WithOne(pi => pi.VentaItem)  
                .HasForeignKey(pi => pi.VentaItemId);
            modelBuilder.Entity<ProgresoLeccion>()
                .HasOne(pl => pl.Estudiante)
                .WithMany()
                .HasForeignKey(pl => pl.EstudianteId);

            modelBuilder.Entity<ProgresoLeccion>()
                .HasOne(pl => pl.Leccion)
                .WithMany()
                .HasForeignKey(pl => pl.LeccionId);
            modelBuilder.Entity<ArtistaFoto>()
                .HasOne(af => af.Artista)
                .WithMany(a => a.Portafolio)
                .HasForeignKey(af => af.ArtistaId);
            modelBuilder.Entity<Evento>()
                .HasOne(e => e.TipoEvento)
                .WithMany(te => te.Eventos)
                .HasForeignKey(e => e.TipoEventoId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<EventoFotos>()
                .HasOne(ef => ef.Evento)
                .WithMany(e => e.Fotos)
                .HasForeignKey(ef => ef.EventoId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RecursoActividad>()
                .HasMany(ra => ra.Entregas)
                .WithOne(ea => ea.RecursoActividad)
                .HasForeignKey(ea => ea.RecursoActividadId); 

            modelBuilder.Entity<EntregaActividad>()
                .HasOne(ea => ea.Estudiante)
                .WithMany()
                .HasForeignKey(ea => ea.EstudianteId);

            modelBuilder.Entity<EntregaActividad>()
                .HasOne(ea => ea.Instructor)
                .WithMany()
                .HasForeignKey(ea => ea.InstructorId);
        }
    }
}