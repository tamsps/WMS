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
        var token = _apiService.GetAccessToken();
        
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Access token not found in session. Redirecting to login.");
            TempData["InfoMessage"] = "Please log in to continue.";
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
            // Fetch Product Statistics
            var productResult = await _apiService.GetAsync<ApiResponse<PagedResult<ProductViewModel>>>("products?pageSize=1");
            if (productResult?.IsSuccess == true && productResult.Data != null)
            {
                model.TotalProducts = productResult.Data.TotalCount;
            }

            // Fetch Location Statistics
            var locationResult = await _apiService.GetAsync<ApiResponse<PagedResult<LocationViewModel>>>("locations?pageSize=1");
            if (locationResult?.IsSuccess == true && locationResult.Data != null)
            {
                model.TotalLocations = locationResult.Data.TotalCount;
            }

            // Fetch Inventory Statistics  
            var inventoryResult = await _apiService.GetAsync<ApiResponse<PagedResult<InventoryViewModel>>>("inventory?pageSize=1000");
            if (inventoryResult?.IsSuccess == true && inventoryResult.Data != null)
            {
                // Calculate total inventory quantity (use QuantityOnHand)
                model.TotalInventoryValue = inventoryResult.Data.Items.Sum(i => i.QuantityOnHand);
            }

            // Fetch Inbound Count - Get total count from list endpoint
            var inboundResult = await _apiService.GetAsync<ApiResponse<PagedResult<InboundViewModel>>>("inbound?pageSize=1");
            if (inboundResult?.IsSuccess == true && inboundResult.Data != null)
            {
                model.PendingInbound = inboundResult.Data.TotalCount;
            }

            // Fetch Outbound Count - Get total count from list endpoint
            var outboundResult = await _apiService.GetAsync<ApiResponse<PagedResult<OutboundViewModel>>>("outbound?pageSize=1");
            if (outboundResult?.IsSuccess == true && outboundResult.Data != null)
            {
                model.PendingOutbound = outboundResult.Data.TotalCount;
            }

            // Fetch Delivery Statistics - Get total count from list endpoint
            var deliveryResult = await _apiService.GetAsync<ApiResponse<PagedResult<DeliveryViewModel>>>("delivery?pageSize=1");
            if (deliveryResult?.IsSuccess == true && deliveryResult.Data != null)
            {
                model.InTransitDeliveries = deliveryResult.Data.TotalCount;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard statistics");
            TempData["WarningMessage"] = "Some statistics could not be loaded. Please refresh the page.";
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
