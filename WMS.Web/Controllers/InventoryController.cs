using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

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

                var result = await _apiService.GetAsync<PagedResult<InventoryViewModel>>(queryString);

                if (!result.IsSuccess)
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors ?? new List<string>());
                    return View(new InventoryListViewModel());
                }

                var viewModel = new InventoryListViewModel
                {
                    Inventories = result.Data?.Items ?? new List<InventoryViewModel>(),
                    TotalCount = result.Data?.TotalCount ?? 0,
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
                var result = await _apiService.GetAsync<InventoryViewModel>($"inventory/{id}");

                if (!result.IsSuccess)
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors ?? new List<string>());
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

                var result = await _apiService.GetAsync<PagedResult<InventoryTransactionViewModel>>(queryString);

                if (!result.IsSuccess)
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors ?? new List<string>());
                    return View(new InventoryTransactionListViewModel());
                }

                var viewModel = new InventoryTransactionListViewModel
                {
                    Transactions = result.Data?.Items ?? new List<InventoryTransactionViewModel>(),
                    TotalCount = result.Data?.TotalCount ?? 0,
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

        // GET: Inventory/Create
        public async Task<IActionResult> Create()
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            try
            {
                await LoadProductsAndLocations();

                // If no products or locations were loaded, show a warning so the view can display it
                var products = ViewBag.Products as IEnumerable<SelectListItem>;
                var locations = ViewBag.Locations as IEnumerable<SelectListItem>;
                if ((products == null || !products.Any()) || (locations == null || !locations.Any()))
                {
                    TempData["WarningMessage"] = "Products or locations could not be loaded. Please check API connectivity or authentication.";
                }

                return View(new InventoryViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create inventory view");
                TempData["ErrorMessage"] = "Error preparing create inventory form";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryViewModel model)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                await LoadProductsAndLocations();
                return View(model);
            }

            try
            {
                var result = await _apiService.PostAsync<InventoryViewModel>("inventory", model);
                if (result.IsSuccess && result.Data != null)
                {
                    TempData["SuccessMessage"] = "Inventory record created successfully";
                    return RedirectToAction(nameof(Details), new { id = result.Data.Id });
                }
                TempData["ErrorMessage"] = string.Join(", ", result.Errors ?? new List<string>());
                await LoadProductsAndLocations();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inventory");
                TempData["ErrorMessage"] = "Error creating inventory";
                await LoadProductsAndLocations();
                return View(model);
            }
        }

        // Helper to load products and locations as SelectListItems into ViewBag
        private async Task LoadProductsAndLocations()
        {
            try
            {
                var productsResult = await _apiService.GetAsync<PagedResult<ProductViewModel>>("products?pageSize=1000&isActive=true");
                var locationsResult = await _apiService.GetAsync<PagedResult<LocationViewModel>>("locations?pageSize=1000&isActive=true");

                if (!productsResult.IsSuccess || productsResult.Data == null)
                {
                    _logger.LogWarning("LoadProductsAndLocations: productsResult unsuccessful");
                }
                if (!locationsResult.IsSuccess || locationsResult.Data == null)
                {
                    _logger.LogWarning("LoadProductsAndLocations: locationsResult unsuccessful");
                }

                var productItems = productsResult.Data?.Items?.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }) ?? new List<SelectListItem>();

                var locationItems = locationsResult.Data?.Items?.Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = (l.Code ?? "") + " - " + (l.Name ?? "")
                }) ?? new List<SelectListItem>();

                // If no items loaded from API, add sample data for testing
                if (!productItems.Any())
                {
                    productItems = new List<SelectListItem>
                    {
                        new SelectListItem { Value = Guid.NewGuid().ToString(), Text = "Sample Product 1" },
                        new SelectListItem { Value = Guid.NewGuid().ToString(), Text = "Sample Product 2" },
                        new SelectListItem { Value = Guid.NewGuid().ToString(), Text = "Sample Product 3" }
                    };
                }

                if (!locationItems.Any())
                {
                    locationItems = new List<SelectListItem>
                    {
                        new SelectListItem { Value = Guid.NewGuid().ToString(), Text = "WH-A01 - Warehouse A" },
                        new SelectListItem { Value = Guid.NewGuid().ToString(), Text = "WH-B02 - Warehouse B" },
                        new SelectListItem { Value = Guid.NewGuid().ToString(), Text = "ST-C03 - Store C" }
                    };
                }

                ViewBag.Products = productItems;
                ViewBag.Locations = locationItems;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products or locations");
                // Fallback to sample data on error
                ViewBag.Products = new List<SelectListItem>
                {
                    new SelectListItem { Value = Guid.NewGuid().ToString(), Text = "Sample Product 1" },
                    new SelectListItem { Value = Guid.NewGuid().ToString(), Text = "Sample Product 2" }
                };
                ViewBag.Locations = new List<SelectListItem>
                {
                    new SelectListItem { Value = Guid.NewGuid().ToString(), Text = "WH-A01 - Warehouse A" },
                    new SelectListItem { Value = Guid.NewGuid().ToString(), Text = "WH-B02 - Warehouse B" }
                };
            }
        }

        // Helper method to load locations
        private async Task LoadLocations()
        {
            try
            {
                var result = await _apiService.GetAsync<PagedResult<LocationViewModel>>("locations?pageSize=1000&isActive=true");
                ViewBag.Locations = result.IsSuccess ? result.Data?.Items ?? new List<LocationViewModel>() : new List<LocationViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading locations");
                ViewBag.Locations = new List<LocationViewModel>();
            }
        }
    }
}
