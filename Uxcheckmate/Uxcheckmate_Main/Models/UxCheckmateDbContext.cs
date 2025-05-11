using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Migrations;

namespace Uxcheckmate_Main.Models;

public partial class UxCheckmateDbContext : DbContext
{
    public UxCheckmateDbContext()
    {
    }

    public UxCheckmateDbContext(DbContextOptions<UxCheckmateDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessibilityCategory> AccessibilityCategories { get; set; }

    public virtual DbSet<AccessibilityIssue> AccessibilityIssues { get; set; }

    public virtual DbSet<DesignCategory> DesignCategories { get; set; }

    public virtual DbSet<DesignIssue> DesignIssues { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<FontLegibility> FontLegibility { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder){
            if (!optionsBuilder.IsConfigured){
                optionsBuilder.UseSqlServer("Name=DBConnection");
        }
    }
   /* protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=DBConnection");*/

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessibilityCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Accessib__3214EC274D88B479");
        });
        // Seeding AccessibilityCategory
        modelBuilder.Entity<AccessibilityCategory>().HasData(
        new AccessibilityCategory { Id = 1, Name = "Color & Contrast", Description = "Issues related to color contrast and visual accessibility." },
        new AccessibilityCategory { Id = 2, Name = "Keyboard & Focus", Description = "Problems with keyboard navigation and focus management." },
        new AccessibilityCategory { Id = 3, Name = "Page Structure & Landmarks", Description = "Issues with headings, ARIA landmarks, and document structure." },
        new AccessibilityCategory { Id = 4, Name = "Forms & Inputs", Description = "Issues with forms, labels, and input fields." },
        new AccessibilityCategory { Id = 5, Name = "Link & Buttons", Description = "Problems with links, buttons, and interactive elements." },
        new AccessibilityCategory { Id = 6, Name = "Multimedia & Animations", Description = "Issues related to videos, audio, images, and animations." },
        new AccessibilityCategory { Id = 7, Name = "Timeouts & Auto-Refresh", Description = "Problems with session timeouts, auto-refreshing pages, and dynamic content updates." },
        new AccessibilityCategory { Id = 8, Name = "Motion & Interaction", Description = "Issues related to animations, scrolling, and movement." },
        new AccessibilityCategory { Id = 9, Name = "ARIA & Semantic HTML", Description = "Issues with incorrect or missing ARIA roles and attributes." },
        new AccessibilityCategory { Id = 10, Name = "Other", Description = "Unknown or experimental WCAG violations" }
        );

        OnModelCreatingPartial(modelBuilder);
        
        modelBuilder.Entity<AccessibilityIssue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Accessib__3214EC2759D39646");

            entity.HasOne(d => d.Category).WithMany(p => p.AccessibilityIssues).HasConstraintName("FK_AccessibilityIssue_CategoryID");

            entity.HasOne(d => d.Report).WithMany(p => p.AccessibilityIssues).HasConstraintName("FK_AccessibilityIssue_ReportID");
        });

        modelBuilder.Entity<DesignCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DesignCa__3214EC27E2402227");
        });

        modelBuilder.Entity<DesignIssue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DesignIs__3214EC277C5F35C0");

            entity.HasOne(d => d.Category).WithMany(p => p.DesignIssues).HasConstraintName("FK_DesignIssue_CategoryID");

            entity.HasOne(d => d.Report).WithMany(p => p.DesignIssues).HasConstraintName("FK_DesignIssue_ReportID");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Report__3214EC27DC95E762");
        });

                    // Font Legibility
        modelBuilder.Entity<FontLegibility>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_FontLegibility");
            entity.Property(e => e.FontName).IsRequired();
            entity.Property(e => e.IsLegible).HasDefaultValue(true);
        });

        // Seeding Illegible Fonts
        modelBuilder.Entity<FontLegibility>().HasData(
            new FontLegibility { Id = 1, FontName = "Chiller", IsLegible = false },
            new FontLegibility { Id = 2, FontName = "Vivaldi", IsLegible = false },
            new FontLegibility { Id = 3, FontName = "Old English Text", IsLegible = false },
            new FontLegibility { Id = 4, FontName = "Jokerman", IsLegible = false },
            new FontLegibility { Id = 5, FontName = "Brush Script", IsLegible = false },
            new FontLegibility { Id = 6, FontName = "Bleeding Cowboys", IsLegible = false },
            new FontLegibility { Id = 7, FontName = "Curlz MT", IsLegible = false },
            new FontLegibility { Id = 8, FontName = "Wingdings", IsLegible = false },
            new FontLegibility { Id = 9, FontName = "Zapfino", IsLegible = false },
            new FontLegibility { Id = 10, FontName = "TrashHand", IsLegible = false },
            new FontLegibility { Id= 11, FontName = "Comic Sans", IsLegible = false }
        );

        OnModelCreatingPartial(modelBuilder);
    }
    public virtual DbSet<UserFeedback> UserFeedbacks { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}