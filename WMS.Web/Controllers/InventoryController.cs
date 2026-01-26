using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IApiService apiService, ILogger<InventoryController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Inventory
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20, string? searchTerm = null, string? filterLocation = null)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var queryString = $"inventory?pageNumber={pageNumber}&pageSize={pageSize}";
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                if (!string.IsNullOrWhiteSpace(filterLocation))
                    queryString += $"&locationId={filterLocation}";

                var result = await _apiService.GetAsync<ApiResponse<PagedResult<InventoryViewModel>>>(queryString);

                var viewModel = new InventoryListViewModel
                {
                    Inventories = result?.Data?.Items ?? new List<InventoryViewModel>(),
                    TotalCount = result?.Data?.TotalCount ?? 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    FilterLocation = filterLocation
                };

                // Load locations for filter dropdown
                await LoadLocations();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory");
                TempData["ErrorMessage"] = "Error loading inventory";
                return View(new InventoryListViewModel());
            }
        }

        // GET: Inventory/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _apiService.GetAsync<ApiResponse<InventoryViewModel>>($"inventory/{id}");

                if (result?.Data == null)
                {
                    TempData["ErrorMessage"] = "Inventory record not found";
                    return RedirectToAction(nameof(Index));
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory details");
                TempData["ErrorMessage"] = "Error loading inventory details";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Inventory/Transactions
        public async Task<IActionResult> Transactions(Guid? inventoryId = null, int pageNumber = 1, int pageSize = 20, string? filterType = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var queryString = $"inventory/transactions?pageNumber={pageNumber}&pageSize={pageSize}";
                if (inventoryId.HasValue)
                    queryString += $"&inventoryId={inventoryId}";
                if (!string.IsNullOrWhiteSpace(filterType))
                    queryString += $"&transactionType={Uri.EscapeDataString(filterType)}";
                if (startDate.HasValue)
                    queryString += $"&startDate={startDate.Value:yyyy-MM-dd}";
                if (endDate.HasValue)
                    queryString += $"&endDate={endDate.Value:yyyy-MM-dd}";

                var result = await _apiService.GetAsync<ApiResponse<PagedResult<InventoryTransactionViewModel>>>(queryString);

                var viewModel = new InventoryTransactionListViewModel
                {
                    Transactions = result?.Data?.Items ?? new List<InventoryTransactionViewModel>(),
                    TotalCount = result?.Data?.TotalCount ?? 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    FilterType = filterType,
                    StartDate = startDate,
                    EndDate = endDate
                };

                ViewBag.InventoryId = inventoryId;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory transactions");
                TempData["ErrorMessage"] = "Error loading inventory transactions";
                return View(new InventoryTransactionListViewModel());
            }
        }

        // Helper method to load locations
        private async Task LoadLocations()
        {
            try
            {
                var result = await _apiService.GetAsync<ApiResponse<PagedResult<LocationViewModel>>>("locations?pageSize=1000&isActive=true");
                ViewBag.Locations = result?.Data?.Items ?? new List<LocationViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading locations");
                ViewBag.Locations = new List<LocationViewModel>();
            }
        }
    }
}
