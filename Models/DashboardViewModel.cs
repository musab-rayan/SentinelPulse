namespace SentinelPulse.Models;

public class DashboardViewModel
{
    public int ActiveCases { get; set; }
    public int PendingFIRs { get; set; }
    public int TotalOfficers { get; set; }
    public int ClosedCases { get; set; }
    public List<FIRModel> RecentFIRs { get; set; } = new();
    public Dictionary<string, int> CrimeDistribution { get; set; } = new();
}
