using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Uxcheckmate_Main.Models;

[Table("DesignIssue")]
public partial class DesignIssue
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

    public int Severity { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("DesignIssues")]
    public virtual DesignCategory Category { get; set; } = null!;

    [ForeignKey("ReportId")]
    [InverseProperty("DesignIssues")]
    public virtual Report Report { get; set; } = null!;
}
