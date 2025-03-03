using System;
using System.Collections.Generic;

namespace Uxcheckmate_Main.Models
{
    public class AxeCoreResults
    {
        public List<AxeViolation> Violations { get; set; }
    }

    public class AxeViolation
    {
        public string Id { get; set; }
        public string Impact { get; set; }
        public string Description { get; set; }
        public string Help { get; set; }
        public List<AxeNode> Nodes { get; set; }
        public List<string> WcagTags { get; set; }
    }

    public class AxeNode
    {

        public List<string> Target { get; set; }

        public string Html { get; set; }  

        public string FailureSummary { get; set; }
    }

}