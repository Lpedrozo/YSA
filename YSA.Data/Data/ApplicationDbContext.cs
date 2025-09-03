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
        }
    }
}