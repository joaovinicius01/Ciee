using Ciee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ciee.Infrastructure.Data;

public class CieeDbContext : DbContext
{
    public DbSet<Candidato> Candidatos => Set<Candidato>();

    public CieeDbContext(DbContextOptions<CieeDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Candidato>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.NomeCompleto)
                .IsRequired()
                .HasMaxLength(150);

            // Mapeia o Value Object Email para uma coluna string no banco
            entity.Property(c => c.Email)
                .HasConversion(
                    email => email.Endereco,
                    endereco => new Ciee.Domain.ValueObjects.Email(endereco))
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(c => c.Telefone)
                .HasMaxLength(20);

            entity.Property(c => c.AreaInteresse)
                .HasMaxLength(100);

            entity.Property(c => c.ResumoProfissional)
                .HasColumnType("NVARCHAR(MAX)");

            entity.Property(c => c.DataCriacao)
                .IsRequired();
        });
    }
}