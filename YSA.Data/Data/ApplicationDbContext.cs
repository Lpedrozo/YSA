using Microsoft.EntityFrameworkCore;
using YSA.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Linq;
using YSA.Core.Enums;

namespace YSA.Data.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario, Rol, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<TasaBCV> TasasBCV { get; set; }
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
        public DbSet<CursoInstructor> CursoInstructores { get; set; }
        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<ArticuloFoto> ArticuloFotos { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<TipoNotificacion> TipoNotificaciones { get; set; }

        // NUEVOS DbSets para cursos presenciales
        public DbSet<ClasePresencial> ClasesPresenciales { get; set; }
        public DbSet<InscripcionClase> InscripcionesClases { get; set; }
        public DbSet<Paquete> Paquetes { get; set; }
        public DbSet<PaqueteCurso> PaqueteCursos { get; set; }
        public DbSet<PaqueteProducto> PaqueteProductos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            // ==================== CONFIGURACIONES EXISTENTES ====================

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

            modelBuilder.Entity<VentaItem>()
                .HasOne(vi => vi.Curso)
                .WithMany()
                .HasForeignKey(vi => vi.CursoId);

            modelBuilder.Entity<VentaItem>()
                .HasOne(vi => vi.Producto)
                .WithMany()
                .HasForeignKey(vi => vi.ProductoId);

            modelBuilder.Entity<Resena>()
                .HasKey(r => new { r.EstudianteId, r.CursoId });

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

            modelBuilder.Entity<Anuncio>()
                .HasOne(a => a.Curso)
                .WithMany(c => c.Anuncios)
                .HasForeignKey(a => a.CursoId);

            modelBuilder.Entity<Resena>()
                .HasOne(r => r.Estudiante)
                .WithMany()
                .HasForeignKey(r => r.EstudianteId);

            modelBuilder.Entity<Resena>()
                .HasOne(r => r.Curso)
                .WithMany(c => c.Resenas)
                .HasForeignKey(r => r.CursoId);

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

            modelBuilder.Entity<CursoInstructor>()
                .HasKey(ci => new { ci.CursoId, ci.ArtistaId });

            modelBuilder.Entity<CursoInstructor>()
                .HasOne(ci => ci.Curso)
                .WithMany(c => c.CursoInstructores)
                .HasForeignKey(ci => ci.CursoId);

            modelBuilder.Entity<CursoInstructor>()
                .HasOne(ci => ci.Artista)
                .WithMany(a => a.CursosInstructores)
                .HasForeignKey(ci => ci.ArtistaId);

            modelBuilder.Entity<ArticuloFoto>()
                .HasOne(af => af.Articulo)
                .WithMany(a => a.Fotos)
                .HasForeignKey(af => af.ArticuloId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notificacion>(entity =>
            {
                entity.HasOne(n => n.Usuario)
                      .WithMany()
                      .HasForeignKey(n => n.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.TipoNotificacion)
                      .WithMany(tn => tn.Notificaciones)
                      .HasForeignKey(n => n.TipoNotificacionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== NUEVAS CONFIGURACIONES PARA CURSOS PRESENCIALES ====================

            // Configuración para Curso - añadir discriminador TipoCurso
            modelBuilder.Entity<Curso>(entity =>
            {
                // Convertir el enum a int para la base de datos, pero sin default value de esta forma
                entity.Property(e => e.TipoCurso)
                    .HasConversion<int>();

                // Opción 1: Usando el valor del enum directamente (recomendado)
                entity.Property(e => e.TipoCurso)
                    .HasDefaultValue(TipoCurso.Digital);

                // Opción 2: Si la opción 1 no funciona, usa el valor numérico casteado
                // entity.Property(e => e.TipoCurso)
                //     .HasDefaultValue((int)TipoCurso.Digital);

                // Configurar la relación con ClasesPresenciales
                entity.HasMany(c => c.ClasesPresenciales)
                    .WithOne(cp => cp.Curso)
                    .HasForeignKey(cp => cp.CursoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración para ClasePresencial
            modelBuilder.Entity<ClasePresencial>(entity =>
            {
                entity.ToTable("ClasesPresenciales");

                entity.HasKey(cp => cp.Id);

                entity.Property(cp => cp.Titulo)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(cp => cp.Descripcion)
                    .HasMaxLength(1000);

                entity.Property(cp => cp.Lugar)
                    .HasMaxLength(255)
                    .HasDefaultValue("Estudio de la Academia");

                entity.Property(cp => cp.Estado)
                    .HasMaxLength(50)
                    .HasDefaultValue("Programada");

                entity.Property(cp => cp.UrlMeet)
                    .HasMaxLength(500);

                // Relación con Curso
                entity.HasOne(cp => cp.Curso)
                    .WithMany(c => c.ClasesPresenciales)
                    .HasForeignKey(cp => cp.CursoId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación con Inscripciones
                entity.HasMany(cp => cp.Inscripciones)
                    .WithOne(i => i.ClasePresencial)
                    .HasForeignKey(i => i.ClasePresencialId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices para búsquedas frecuentes
                entity.HasIndex(cp => cp.CursoId);
                entity.HasIndex(cp => cp.FechaHoraInicio);
                entity.HasIndex(cp => cp.Estado);
            });

            // Configuración para InscripcionClase
            modelBuilder.Entity<InscripcionClase>(entity =>
            {
                entity.ToTable("InscripcionesClases");

                entity.HasKey(i => i.Id);

                entity.Property(i => i.EstadoAsistencia)
                    .HasMaxLength(50)
                    .HasDefaultValue("Pendiente");

                entity.Property(i => i.Comentario)
                    .HasMaxLength(500);

                // Relación con ClasePresencial
                entity.HasOne(i => i.ClasePresencial)
                    .WithMany(cp => cp.Inscripciones)
                    .HasForeignKey(i => i.ClasePresencialId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación con Usuario (Estudiante)
                entity.HasOne(i => i.Estudiante)
                    .WithMany(u => u.InscripcionesClases)
                    .HasForeignKey(i => i.EstudianteId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índices para búsquedas frecuentes
                entity.HasIndex(i => i.ClasePresencialId);
                entity.HasIndex(i => i.EstudianteId);
                entity.HasIndex(i => new { i.ClasePresencialId, i.EstudianteId }).IsUnique(); // Evita doble inscripción
                entity.HasIndex(i => i.EstadoAsistencia);
            });

            // Configuración para Usuario - nuevos campos
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.Property(u => u.Cedula)
                    .HasMaxLength(20);

                entity.Property(u => u.WhatsApp)
                    .HasMaxLength(20);

                entity.Property(u => u.ExperienciaTatuaje)
                    .HasMaxLength(500);

                entity.Property(u => u.NombreRepresentante)
                    .HasMaxLength(255);

                entity.Property(u => u.CedulaRepresentante)
                    .HasMaxLength(20);

                entity.Property(u => u.AtendidoPor)
                    .HasMaxLength(100);

                // Índice para búsqueda por cédula (útil para encontrar estudiantes)
                entity.HasIndex(u => u.Cedula);

                // Índice para búsqueda por WhatsApp
                entity.HasIndex(u => u.WhatsApp);

                // Relación con InscripcionesClases
                entity.HasMany(u => u.InscripcionesClases)
                    .WithOne(i => i.Estudiante)
                    .HasForeignKey(i => i.EstudianteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración adicional para RecursoActividad - asegurar que puede usarse con ClasePresencial
            modelBuilder.Entity<RecursoActividad>(entity =>
            {
                entity.HasIndex(r => new { r.TipoEntidad, r.EntidadId });
            });
            // ==================== CONFIGURACIONES PARA PAQUETES ====================

            // Configuración para Paquete
            modelBuilder.Entity<Paquete>(entity =>
            {
                entity.ToTable("Paquetes");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Titulo)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(p => p.DescripcionCorta)
                    .HasMaxLength(255);

                entity.Property(p => p.Precio)
                    .HasColumnType("decimal(10,2)");

                entity.Property(p => p.UrlImagen)
                    .HasMaxLength(255);

                // Índices
                entity.HasIndex(p => p.EsDestacado);
                entity.HasIndex(p => p.FechaPublicacion);
            });

            // Configuración para PaqueteCurso (tabla intermedia)
            modelBuilder.Entity<PaqueteCurso>(entity =>
            {
                entity.ToTable("PaqueteCursos");

                entity.HasKey(pc => new { pc.PaqueteId, pc.CursoId });

                entity.HasOne(pc => pc.Paquete)
                    .WithMany(p => p.PaqueteCursos)
                    .HasForeignKey(pc => pc.PaqueteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pc => pc.Curso)
                    .WithMany()
                    .HasForeignKey(pc => pc.CursoId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(pc => pc.PaqueteId);
                entity.HasIndex(pc => pc.CursoId);
            });

            // Configuración para PaqueteProducto (tabla intermedia)
            modelBuilder.Entity<PaqueteProducto>(entity =>
            {
                entity.ToTable("PaqueteProductos");

                entity.HasKey(pp => new { pp.PaqueteId, pp.ProductoId });

                entity.HasOne(pp => pp.Paquete)
                    .WithMany(p => p.PaqueteProductos)
                    .HasForeignKey(pp => pp.PaqueteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pp => pp.Producto)
                    .WithMany()
                    .HasForeignKey(pp => pp.ProductoId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(pp => pp.PaqueteId);
                entity.HasIndex(pp => pp.ProductoId);
            });

            // Modificar VentaItem para incluir PaqueteId
            modelBuilder.Entity<VentaItem>(entity =>
            {
                entity.HasOne(vi => vi.Curso)
                    .WithMany()
                    .HasForeignKey(vi => vi.CursoId);

                entity.HasOne(vi => vi.Producto)
                    .WithMany()
                    .HasForeignKey(vi => vi.ProductoId);

                // NUEVA relación con Paquete
                entity.HasOne(vi => vi.Paquete)
                    .WithMany()
                    .HasForeignKey(vi => vi.PaqueteId);

                // Índice para búsqueda por tipo
                entity.HasIndex(vi => vi.Tipo);

                // Índices para las claves foráneas
                entity.HasIndex(vi => vi.CursoId);
                entity.HasIndex(vi => vi.ProductoId);
                entity.HasIndex(vi => vi.PaqueteId);
            });
        }
    }
}