using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Uxcheckmate_Main.Models;

[Table("DesignReport")]
public partial class DesignReport
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string Url { get; set; }

    public virtual ICollection<DesignIssue> DesignScanResults { get; set; } = new List<DesignIssue>();
}
