using System;
using System.Collections.Generic;
using IssueTracker.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Infrastructure.Context;

public partial class IssueTrackerDbContext : DbContext
{
    public IssueTrackerDbContext(DbContextOptions<IssueTrackerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Issue> Issues { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Issue>(entity =>
        {
            entity.HasKey(e => e.IssueId).HasName("PK__Issues__6C861604B8532DAE");

            entity.HasIndex(e => e.IssueKey, "UQ__Issues__B3A72BED15E3A277").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IssueKey).HasMaxLength(20);
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValue("Medium");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Backlog");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Assignee).WithMany(p => p.IssueAssignees)
                .HasForeignKey(d => d.AssigneeId)
                .HasConstraintName("FK__Issues__Assignee__571DF1D5");

            entity.HasOne(d => d.Project).WithMany(p => p.Issues)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK__Issues__ProjectI__534D60F1");

            entity.HasOne(d => d.Reporter).WithMany(p => p.IssueReporters)
                .HasForeignKey(d => d.ReporterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Issues__Reporter__5629CD9C");

            entity.HasMany(d => d.Tags).WithMany(p => p.Issues)
                .UsingEntity<Dictionary<string, object>>(
                    "IssueTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("FK__IssueTags__TagId__5FB337D6"),
                    l => l.HasOne<Issue>().WithMany()
                        .HasForeignKey("IssueId")
                        .HasConstraintName("FK__IssueTags__Issue__5EBF139D"),
                    j =>
                    {
                        j.HasKey("IssueId", "TagId").HasName("PK__IssueTag__BAD1D99E00A4D950");
                        j.ToTable("IssueTags");
                    });
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__Projects__761ABEF08073D506");

            entity.HasIndex(e => e.KeyPrefix, "UQ__Projects__F5583C370F4B2811").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.KeyPrefix).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__Tags__657CF9AC1F270891");

            entity.HasIndex(e => e.Name, "UQ__Tags__737584F639908945").IsUnique();

            entity.Property(e => e.ColorHex)
                .HasMaxLength(7)
                .HasDefaultValue("#6c757d");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CA049F160");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534163632FE").IsUnique();

            entity.Property(e => e.AvatarUrl).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(120);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
