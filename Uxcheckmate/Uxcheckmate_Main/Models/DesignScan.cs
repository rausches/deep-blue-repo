using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Uxcheckmate_Main.Models;

[Table("DesignScan")]
public partial class DesignScan
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [Column("URL")]
    public required string Url { get; set; }

    public virtual ICollection<DesignIssue> DesignScanResults { get; set; } = new List<DesignIssue>();
}
