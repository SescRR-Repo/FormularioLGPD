// Data/ApplicationDbContext.cs
using FormularioLGPD.Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FormularioLGPD.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Titular> Titulares { get; set; }
        public DbSet<Dependente> Dependentes { get; set; }
        public DbSet<TermoAceite> TermosAceite { get; set; }
        public DbSet<LogAuditoria> LogsAuditoria { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da entidade Titular
            modelBuilder.Entity<Titular>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.CPF).IsUnique();
                entity.Property(t => t.DataCadastro).HasDefaultValueSql("GETUTCDATE()");

                // Relacionamento um-para-muitos com Dependente
                entity.HasMany(t => t.Dependentes)
                      .WithOne(d => d.Titular)
                      .HasForeignKey(d => d.TitularId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relacionamento um-para-um com TermoAceite
                entity.HasOne(t => t.TermoAceite)
                      .WithOne(ta => ta.Titular)
                      .HasForeignKey<TermoAceite>(ta => ta.TitularId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração da entidade Dependente
            modelBuilder.Entity<Dependente>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.HasIndex(d => new { d.TitularId, d.CPF }).IsUnique();
                entity.Property(d => d.DataCadastro).HasDefaultValueSql("GETUTCDATE()");

                // Computed column para IsMenorIdade não é necessária pois é uma propriedade calculada
                entity.Ignore(d => d.IsMenorIdade);
            });

            // Configuração da entidade TermoAceite
            modelBuilder.Entity<TermoAceite>(entity =>
            {
                entity.HasKey(ta => ta.Id);
                entity.HasIndex(ta => ta.NumeroTermo).IsUnique();
                entity.HasIndex(ta => ta.DataHoraAceite);
                entity.Property(ta => ta.DataCriacao).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(ta => ta.ConteudoTermo).HasColumnType("nvarchar(max)");
            });

            // Configuração da entidade LogAuditoria
            modelBuilder.Entity<LogAuditoria>(entity =>
            {
                entity.HasKey(la => la.Id);
                entity.HasIndex(la => la.DataHoraOperacao);
                entity.HasIndex(la => la.TipoOperacao);
                entity.Property(la => la.DadosAntes).HasColumnType("nvarchar(max)");
                entity.Property(la => la.DadosDepois).HasColumnType("nvarchar(max)");

                // Relacionamento opcional com TermoAceite
                entity.HasOne(la => la.TermoAceite)
                      .WithMany()
                      .HasForeignKey(la => la.TermoAceiteId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configurações dos Enums
            modelBuilder.Entity<Titular>()
                .Property(t => t.QualificacaoLegal)
                .HasConversion<int>();

            modelBuilder.Entity<TermoAceite>()
                .Property(ta => ta.StatusTermo)
                .HasConversion<int>();

            modelBuilder.Entity<LogAuditoria>()
                .Property(la => la.StatusOperacao)
                .HasConversion<int>();
        }
    }
}
