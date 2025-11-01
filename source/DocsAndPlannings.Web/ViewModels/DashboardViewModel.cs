namespace DocsAndPlannings.Web.ViewModels;

public class DashboardViewModel
{
    public string UserName { get; set; } = string.Empty;
    public DashboardStats Stats { get; set; } = new DashboardStats();
    public List<RecentActivityItem> RecentActivity { get; set; } = new List<RecentActivityItem>();
}

public class DashboardStats
{
    public int TotalDocuments { get; set; }
    public int TotalProjects { get; set; }
    public int AssignedWorkItems { get; set; }
    public int ActiveWorkItems { get; set; }
}

public class RecentActivityItem
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
