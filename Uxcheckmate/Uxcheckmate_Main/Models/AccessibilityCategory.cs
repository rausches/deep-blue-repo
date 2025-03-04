using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Uxcheckmate_Main.Models;

[Table("AccessibilityCategory")]
public partial class AccessibilityCategory
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(128)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "text")]
    public string? Description { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<AccessibilityIssue> AccessibilityIssues { get; set; } = new List<AccessibilityIssue>();
}
