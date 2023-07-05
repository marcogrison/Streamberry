using Microsoft.EntityFrameworkCore;
using StreamberryAPI.Models;

namespace StreamberryAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<FilmeModel> Filme { get; set; }
        public DbSet<StreamingModel> Streaming { get; set; }
        public DbSet<ComentarioModel> Comentario { get; set; }
        public DbSet<GeneroModel> Genero { get; set; }
        public DbSet<AvaliacaoModel> Avaliacao { get; set; }
        public DbSet<FilmeStreamingModel> FilmeStreaming { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FilmeModel>()
            .HasMany(f => f.Comentarios)
            .WithOne(c => c.Filme)
            .HasForeignKey(c => c.FilmeID);

            modelBuilder.Entity<FilmeModel>()
                .HasMany(f => f.Streamings)
                .WithOne(fs => fs.Filme)
                .HasForeignKey(fs => fs.FilmeID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AvaliacaoModel>()
                .HasOne(a => a.Filme)
                .WithMany(f => f.Avaliacoes)
                .HasForeignKey(a => a.FilmeID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StreamingModel>()
                .HasOne(s => s.Filme)
                .WithMany(f => f.Streamings)
                .HasForeignKey(s => s.FilmeID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComentarioModel>()
                .HasOne(c => c.Filme)
                .WithMany(f => f.Comentarios)
                .HasForeignKey(c => c.FilmeID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GeneroModel>()
                .HasMany(g => g.Filmes)
                .WithOne(f => f.Genero)
                .HasForeignKey(f => f.GeneroId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FilmeStreamingModel>()
            .HasOne(fs => fs.Filme)
            .WithMany(fm => fm.FilmeStreamings) // Adicione esta linha para especificar a propriedade de navegação inversa
            .HasForeignKey(fs => fs.FilmeId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FilmeStreamingModel>()
                .HasOne(fs => fs.Streaming)
                .WithMany() // Remova o uso do método WithMany() neste caso, pois não há uma propriedade de navegação inversa
                .HasForeignKey(fs => fs.StreamingId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
