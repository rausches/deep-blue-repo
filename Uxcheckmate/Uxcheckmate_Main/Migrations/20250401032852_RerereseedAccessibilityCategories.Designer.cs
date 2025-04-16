﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Uxcheckmate_Main.Models;

#nullable disable

namespace Uxcheckmate_Main.Migrations
{
    [DbContext(typeof(UxCheckmateDbContext))]
    [Migration("20250401032852_RerereseedAccessibilityCategories")]
    partial class RerereseedAccessibilityCategories
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Uxcheckmate_Main.Models.AccessibilityCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .IsUnicode(false)
                        .HasColumnType("varchar(128)");

                    b.HasKey("Id")
                        .HasName("PK__Accessib__3214EC274D88B479");

                    b.ToTable("AccessibilityCategory");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "Issues related to color contrast and visual accessibility.",
                            Name = "Color & Contrast"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Problems with keyboard navigation and focus management.",
                            Name = "Keyboard & Focus"
                        },
                        new
                        {
                            Id = 3,
                            Description = "Issues with headings, ARIA landmarks, and document structure.",
                            Name = "Page Structure & Landmarks"
                        },
                        new
                        {
                            Id = 4,
                            Description = "Issues with forms, labels, and input fields.",
                            Name = "Forms & Inputs"
                        },
                        new
                        {
                            Id = 5,
                            Description = "Problems with links, buttons, and interactive elements.",
                            Name = "Link & Buttons"
                        },
                        new
                        {
                            Id = 6,
                            Description = "Issues related to videos, audio, images, and animations.",
                            Name = "Multimedia & Animations"
                        },
                        new
                        {
                            Id = 7,
                            Description = "Problems with session timeouts, auto-refreshing pages, and dynamic content updates.",
                            Name = "Timeouts & Auto-Refresh"
                        },
                        new
                        {
                            Id = 8,
                            Description = "Issues related to animations, scrolling, and movement.",
                            Name = "Motion & Interaction"
                        },
                        new
                        {
                            Id = 9,
                            Description = "Issues with incorrect or missing ARIA roles and attributes.",
                            Name = "ARIA & Semantic HTML"
                        },
                        new
                        {
                            Id = 10,
                            Description = "Unknown or experimental WCAG violations",
                            Name = "Other"
                        });
                });

            modelBuilder.Entity("Uxcheckmate_Main.Models.AccessibilityIssue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("int")
                        .HasColumnName("CategoryID");

                    b.Property<string>("Details")
                        .IsRequired()
                        .HasColumnType("varchar(max)");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ReportId")
                        .HasColumnType("int")
                        .HasColumnName("ReportID");

                    b.Property<string>("Selector")
                        .IsRequired()
                        .IsUnicode(false)
                        .HasColumnType("varchar(max)");

                    b.Property<int>("Severity")
                        .HasColumnType("int");

                    b.Property<string>("WCAG")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id")
                        .HasName("PK__Accessib__3214EC2759D39646");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ReportId");

                    b.ToTable("AccessibilityIssue");
                });

            modelBuilder.Entity("Uxcheckmate_Main.Models.DesignCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .IsUnicode(false)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ScanMethod")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id")
                        .HasName("PK__DesignCa__3214EC27E2402227");

                    b.ToTable("DesignCategory");
                });

            modelBuilder.Entity("Uxcheckmate_Main.Models.DesignIssue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("int")
                        .HasColumnName("CategoryID");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("varchar(max)");

                    b.Property<int>("ReportId")
                        .HasColumnType("int")
                        .HasColumnName("ReportID");

                    b.Property<int>("Severity")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PK__DesignIs__3214EC277C5F35C0");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ReportId");

                    b.ToTable("DesignIssue");
                });

            modelBuilder.Entity("Uxcheckmate_Main.Models.FontLegibility", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FontName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsLegible")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.HasKey("Id")
                        .HasName("PK_FontLegibility");

                    b.ToTable("FontLegibility");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            FontName = "Chiller",
                            IsLegible = false
                        },
                        new
                        {
                            Id = 2,
                            FontName = "Vivaldi",
                            IsLegible = false
                        },
                        new
                        {
                            Id = 3,
                            FontName = "Old English Text",
                            IsLegible = false
                        },
                        new
                        {
                            Id = 4,
                            FontName = "Jokerman",
                            IsLegible = false
                        },
                        new
                        {
                            Id = 5,
                            FontName = "Brush Script",
                            IsLegible = false
                        },
                        new
                        {
                            Id = 6,
                            FontName = "Bleeding Cowboys",
                            IsLegible = false
                        },
                        new
                        {
                            Id = 7,
                            FontName = "Curlz MT",
                            IsLegible = false
                        },
                        new
                        {
                            Id = 8,
                            FontName = "Wingdings",
                            IsLegible = false
                        },
                        new
                        {
                            Id = 9,
                            FontName = "Zapfino",
                            IsLegible = false
                        },
                        new
                        {
                            Id = 10,
                            FontName = "TrashHand",
                            IsLegible = false
                        },
                        new
                        {
                            Id = 11,
                            FontName = "Comic Sans",
                            IsLegible = false
                        });
                });

            modelBuilder.Entity("Uxcheckmate_Main.Models.Report", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(128)
                        .IsUnicode(false)
                        .HasColumnType("varchar(128)")
                        .HasColumnName("URL");

                    b.Property<string>("UserID")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("UserID");

                    b.HasKey("Id")
                        .HasName("PK__Report__3214EC27DC95E762");

                    b.ToTable("Report");
                });

            modelBuilder.Entity("Uxcheckmate_Main.Models.AccessibilityIssue", b =>
                {
                    b.HasOne("Uxcheckmate_Main.Models.AccessibilityCategory", "Category")
                        .WithMany("AccessibilityIssues")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_AccessibilityIssue_CategoryID");

                    b.HasOne("Uxcheckmate_Main.Models.Report", "Report")
                        .WithMany("AccessibilityIssues")
                        .HasForeignKey("ReportId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_AccessibilityIssue_ReportID");

                    b.Navigation("Category");

                    b.Navigation("Report");
                });

            modelBuilder.Entity("Uxcheckmate_Main.Models.DesignIssue", b =>
                {
                    b.HasOne("Uxcheckmate_Main.Models.DesignCategory", "Category")
                        .WithMany("DesignIssues")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_DesignIssue_CategoryID");

                    b.HasOne("Uxcheckmate_Main.Models.Report", "Report")
                        .WithMany("DesignIssues")
                        .HasForeignKey("ReportId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_DesignIssue_ReportID");

                    b.Navigation("Category");

                    b.Navigation("Report");
                });

            modelBuilder.Entity("Uxcheckmate_Main.Models.AccessibilityCategory", b =>
                {
                    b.Navigation("AccessibilityIssues");
                });

            modelBuilder.Entity("Uxcheckmate_Main.Models.DesignCategory", b =>
                {
                    b.Navigation("DesignIssues");
                });

            modelBuilder.Entity("Uxcheckmate_Main.Models.Report", b =>
                {
                    b.Navigation("AccessibilityIssues");

                    b.Navigation("DesignIssues");
                });
#pragma warning restore 612, 618
        }
    }
}
