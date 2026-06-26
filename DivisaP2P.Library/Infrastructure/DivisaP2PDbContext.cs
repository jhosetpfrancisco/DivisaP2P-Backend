using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using DivisaP2P.Library.Core.Entities;

namespace DivisaP2P.Library.Infrastructure;

/// <summary>
/// DbContext de la plataforma P2P de intercambio de divisas.
/// Enfoque Database-First: el mapeo fluido refleja el esquema definido en
/// Database/01_CreateDatabase_PostgreSQL.sql.
/// </summary>
public partial class DivisaP2PDbContext : DbContext
{
    public DivisaP2PDbContext()
    {
    }

    public DivisaP2PDbContext(DbContextOptions<DivisaP2PDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<CuentaBancaria> CuentasBancarias { get; set; }

    public virtual DbSet<Oferta> Ofertas { get; set; }

    public virtual DbSet<Transaccion> Transacciones { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    public virtual DbSet<HistorialEstadoTransaccion> HistorialEstadoTransacciones { get; set; }

    public virtual DbSet<Calificacion> Calificaciones { get; set; }

    public virtual DbSet<Disputa> Disputas { get; set; }

    public virtual DbSet<DisputaEvidencia> DisputaEvidencias { get; set; }

    public virtual DbSet<Notificacion> Notificaciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuario");

            entity.HasIndex(e => e.Correo).IsUnique();

            entity.Property(e => e.Rol).HasMaxLength(3);
            entity.Property(e => e.Nombres).HasMaxLength(100);
            entity.Property(e => e.ApellidoPaterno).HasMaxLength(60);
            entity.Property(e => e.ApellidoMaterno).HasMaxLength(60);
            entity.Property(e => e.RazonSocial).HasMaxLength(150);
            entity.Property(e => e.Ruc).HasMaxLength(11);
            entity.Property(e => e.RepresentanteLegal).HasMaxLength(150);
            entity.Property(e => e.Correo).HasMaxLength(120);
            entity.Property(e => e.PasswordHash).HasMaxLength(200);
            entity.Property(e => e.TipoDocumento).HasMaxLength(3);
            entity.Property(e => e.NumeroDocumento).HasMaxLength(12);
            entity.Property(e => e.Celular).HasMaxLength(20);
            entity.Property(e => e.Estado).HasMaxLength(25);
            entity.Property(e => e.MotivoBloqueo).HasMaxLength(300);
            entity.Property(e => e.CalificacionPromedio).HasColumnType("decimal(3, 2)");
        });

        modelBuilder.Entity<CuentaBancaria>(entity =>
        {
            entity.ToTable("CuentaBancaria");

            entity.Property(e => e.Banco).HasMaxLength(80);
            entity.Property(e => e.TipoCuenta).HasMaxLength(15);
            entity.Property(e => e.Divisa).HasMaxLength(3);
            entity.Property(e => e.NumeroCuenta).HasMaxLength(30);
            entity.Property(e => e.Cci).HasMaxLength(20);
            entity.Property(e => e.NombreTitular).HasMaxLength(150);

            entity.HasOne(d => d.Usuario).WithMany(p => p.CuentasBancarias)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_CuentaBancaria_Usuario");
        });

        modelBuilder.Entity<Oferta>(entity =>
        {
            entity.ToTable("Oferta");

            entity.Property(e => e.TipoOperacion).HasMaxLength(10);
            entity.Property(e => e.DivisaOrigen).HasMaxLength(3);
            entity.Property(e => e.DivisaDestino).HasMaxLength(3);
            entity.Property(e => e.Estado).HasMaxLength(15);
            entity.Property(e => e.MontoTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MontoDisponible).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TipoCambio).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Ofertas)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Oferta_Usuario");

            entity.HasOne(d => d.CuentaBancaria).WithMany()
                .HasForeignKey(d => d.CuentaBancariaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Oferta_CuentaBancaria");
        });

        modelBuilder.Entity<Transaccion>(entity =>
        {
            entity.ToTable("Transaccion");

            entity.HasIndex(e => e.Codigo).IsUnique();

            entity.Property(e => e.Codigo).HasMaxLength(20);
            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.MontoOperado).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TipoCambio).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.Oferta).WithMany(p => p.Transacciones)
                .HasForeignKey(d => d.OfertaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Transaccion_Oferta");

            // Comprador y Vendedor apuntan a la misma tabla Usuario: usar NoAction
            // para evitar múltiples rutas de cascada (error de SQL Server).
            entity.HasOne(d => d.Comprador).WithMany()
                .HasForeignKey(d => d.CompradorId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Transaccion_Comprador");

            entity.HasOne(d => d.Vendedor).WithMany()
                .HasForeignKey(d => d.VendedorId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Transaccion_Vendedor");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.ToTable("Voucher");

            entity.Property(e => e.Tipo).HasMaxLength(10);
            entity.Property(e => e.RutaArchivo).HasMaxLength(300);
            entity.Property(e => e.NombreArchivo).HasMaxLength(200);
            entity.Property(e => e.NumeroOperacion).HasMaxLength(50);

            entity.HasOne(d => d.Transaccion).WithMany(p => p.Vouchers)
                .HasForeignKey(d => d.TransaccionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Voucher_Transaccion");
        });

        modelBuilder.Entity<HistorialEstadoTransaccion>(entity =>
        {
            entity.ToTable("HistorialEstadoTransaccion");

            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.Comentario).HasMaxLength(300);

            entity.HasOne(d => d.Transaccion).WithMany(p => p.Historial)
                .HasForeignKey(d => d.TransaccionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Historial_Transaccion");
        });

        modelBuilder.Entity<Calificacion>(entity =>
        {
            entity.ToTable("Calificacion");

            // Un usuario solo puede calificar una vez por transacción (US-012).
            entity.HasIndex(e => new { e.TransaccionId, e.CalificadorId }).IsUnique();

            entity.Property(e => e.Comentario).HasMaxLength(200);

            entity.HasOne(d => d.Transaccion).WithMany(p => p.Calificaciones)
                .HasForeignKey(d => d.TransaccionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Calificacion_Transaccion");
        });

        modelBuilder.Entity<Disputa>(entity =>
        {
            entity.ToTable("Disputa");

            // Una transacción admite como máximo una disputa.
            entity.HasIndex(e => e.TransaccionId).IsUnique();

            entity.Property(e => e.Motivo).HasMaxLength(1000);
            entity.Property(e => e.Estado).HasMaxLength(15);
            entity.Property(e => e.Resolucion).HasMaxLength(20);
            entity.Property(e => e.ComentarioResolucion).HasMaxLength(1000);

            entity.HasOne(d => d.Transaccion).WithOne(p => p.Disputa)
                .HasForeignKey<Disputa>(d => d.TransaccionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Disputa_Transaccion");
        });

        modelBuilder.Entity<DisputaEvidencia>(entity =>
        {
            entity.ToTable("DisputaEvidencia");

            entity.Property(e => e.RutaArchivo).HasMaxLength(300);
            entity.Property(e => e.NombreArchivo).HasMaxLength(200);

            entity.HasOne(d => d.Disputa).WithMany(p => p.Evidencias)
                .HasForeignKey(d => d.DisputaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DisputaEvidencia_Disputa");
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.ToTable("Notificacion");

            entity.Property(e => e.Titulo).HasMaxLength(150);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Enlace).HasMaxLength(300);

            entity.HasOne(d => d.Usuario).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Notificacion_Usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
