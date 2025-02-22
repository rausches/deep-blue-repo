using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Uxcheckmate_Main.Models;

[Table("DesignCategory")]
public partial class DesignCategory
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(128)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "text")]
    public string? Description { get; set; }

    public string ScanMethod { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<DesignIssue> DesignIssues { get; set; } = new List<DesignIssue>();
}
