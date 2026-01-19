namespace WMS.Web.Models;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalLocations { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int PendingInbound { get; set; }
    public int PendingOutbound { get; set; }
    public int InTransitDeliveries { get; set; }
}
