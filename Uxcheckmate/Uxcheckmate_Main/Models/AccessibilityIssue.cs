using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Uxcheckmate_Main.Models;

[Table("AccessibilityIssue")]
public partial class AccessibilityIssue
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [Column("ReportID")]
    public int ReportId { get; set; }

    [Column(TypeName = "text")]
    public string Message { get; set; } = null!;

    [Column(TypeName= "varchar(max)")]
    public string Details {get; set; }

    [Column(TypeName = "varchar(max)")]
    [Unicode(false)]
    public string Selector { get; set; } = null!;

    public int Severity { get; set; }

    public string WCAG { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("AccessibilityIssues")]
    public virtual AccessibilityCategory Category { get; set; } = null!;

    [ForeignKey("ReportId")]
    [InverseProperty("AccessibilityIssues")]
    public virtual Report Report { get; set; } = null!;
}
