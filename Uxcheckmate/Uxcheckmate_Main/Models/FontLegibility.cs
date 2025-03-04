using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Uxcheckmate_Main.Models
{
    public class FontLegibility
    {
        [Key]
        public int Id { get; set; }
        public string FontName { get; set; } = string.Empty;
        public bool IsLegible { get; set; }
    }
}
