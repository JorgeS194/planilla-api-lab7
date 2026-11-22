using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PlanillaApi.Models;

namespace PlanillaApi.Data;

public partial class PlanillaDbContext : DbContext
{
    public PlanillaDbContext(DbContextOptions<PlanillaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Ubicacion).HasMaxLength(100);
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasIndex(e => e.DepartamentoId, "IX_Empleados_DepartamentoId");

            entity.HasIndex(e => e.Cedula, "UQ_Empleados_Cedula").IsUnique();

            entity.Property(e => e.Apellido).HasMaxLength(60);
            entity.Property(e => e.Cargo).HasMaxLength(80);
            entity.Property(e => e.Cedula).HasMaxLength(20);
            entity.Property(e => e.Nombre).HasMaxLength(60);
            entity.Property(e => e.SalarioBase).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Departamento).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.DepartamentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Empleados_Departamentos");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
