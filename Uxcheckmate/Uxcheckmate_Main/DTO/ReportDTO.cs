namespace Uxcheckmate_Main.DTO;
public class ReportDTO
{
    public int Id { get; set; }
    public string Url { get; set; }
    public DateOnly Date { get; set; }
    public List<DesignIssueDTO> DesignIssues { get; set; }
    public List<AccessibilityIssueDTO> AccessibilityIssues { get; set; }
    public string Summary { get; set; }
}