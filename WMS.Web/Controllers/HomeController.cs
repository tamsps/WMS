using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IApiService _apiService;

    public HomeController(ILogger<HomeController> logger, IApiService apiService)
    {
        _logger = logger;
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        // Check if user is logged in
        if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
        {
            return RedirectToAction("Login", "Account");
        }

        // Fetch statistics from API
        var model = new DashboardViewModel
        {
            TotalProducts = 0,
            TotalLocations = 0,
            TotalInventoryValue = 0,
            PendingInbound = 0,
            PendingOutbound = 0,
            InTransitDeliveries = 0
        };

        try
        {
            // You can fetch actual statistics from API endpoints
            // var productStats = await _apiService.GetAsync<dynamic>("api/products/statistics");
            // model.TotalProducts = productStats?.total ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard statistics");
        }

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
