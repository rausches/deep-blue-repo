public class ReportDTO
{
    public string Url { get; set; }
    public DateOnly Date { get; set; }
    public List<DesignIssueDTO> DesignIssues { get; set; }
    public List<AccessibilityIssueDTO> AccessibilityIssues { get; set; }
}