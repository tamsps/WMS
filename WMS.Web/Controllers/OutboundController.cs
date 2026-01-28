using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers
{
    public class OutboundController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<OutboundController> _logger;

        public OutboundController(IApiService apiService, ILogger<OutboundController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20, string? searchTerm = null, string? filterStatus = null)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            try
            {
                var queryString = $"outbound?pageNumber={pageNumber}&pageSize={pageSize}";
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                if (!string.IsNullOrWhiteSpace(filterStatus))
                    queryString += $"&status={Uri.EscapeDataString(filterStatus)}";

                var result = await _apiService.GetAsync<PagedResult<OutboundViewModel>>(queryString);

                var viewModel = new OutboundListViewModel
                {
                    Items = result.Data?.Items ?? new List<OutboundViewModel>(),
                    TotalCount = result.Data?.TotalCount ?? 0,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    FilterStatus = filterStatus
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading outbounds");
                TempData["ErrorMessage"] = "Error loading outbound orders";
                return View(new OutboundListViewModel());
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            try
            {
                var result = await _apiService.GetAsync<OutboundViewModel>($"outbound/{id}");
                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["ErrorMessage"] = "Outbound order not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading outbound details");
                TempData["ErrorMessage"] = "Error loading outbound order details";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create()
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");
            
            await LoadProductsAndLocations();
            return View(new CreateOutboundViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOutboundViewModel model)
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
                var result = await _apiService.PostAsync<OutboundViewModel>("outbound", model);
                if (result.IsSuccess && result.Data != null)
                {
                    TempData["SuccessMessage"] = "Outbound order created successfully. Please pick the items.";
                    // Redirect to Pick view instead of Details
                    return RedirectToAction(nameof(Pick), new { id = result.Data.Id });
                }
                TempData["ErrorMessage"] = result.Message ?? "Failed to create outbound order";
                await LoadProductsAndLocations();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating outbound order");
                TempData["ErrorMessage"] = "Error creating outbound order";
                await LoadProductsAndLocations();
                return View(model);
            }
        }

        // GET: Outbound/Pick/5
        public async Task<IActionResult> Pick(Guid id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            try
            {
                var result = await _apiService.GetAsync<OutboundViewModel>($"outbound/{id}");
                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["ErrorMessage"] = "Outbound order not found";
                    return RedirectToAction(nameof(Index));
                }

                var pickModel = new PickOutboundViewModel
                {
                    Id = result.Data.Id,
                    OutboundNumber = result.Data.OutboundNumber,
                    OrderNumber = result.Data.OrderNumber,
                    CustomerName = result.Data.CustomerName,
                    Status = result.Data.Status,
                    Items = result.Data.Items.Select(i => new PickOutboundItemViewModel
                    {
                        ItemId = i.Id,
                        ProductId = i.ProductId,
                        ProductSku = i.ProductSku,
                        ProductName = i.ProductName,
                        LocationId = i.LocationId,
                        LocationCode = i.LocationCode,
                        LocationName = i.LocationName,
                        OrderedQuantity = i.OrderedQuantity,
                        PickedQuantity = i.PickedQuantity,
                        AvailableQuantity = i.AvailableQuantity,
                        UOM = i.UOM
                    }).ToList()
                };

                return View(pickModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading outbound for picking");
                TempData["ErrorMessage"] = "Error loading outbound order";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Outbound/Pick/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pick(Guid id, PickOutboundViewModel model)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            try
            {
                // Transform to DTO expected by API
                var pickDto = new PickOutboundDto
                {
                    OutboundId = model.Id,
                    Items = model.Items.Select(item => new PickOutboundItemDto
                    {
                        OutboundItemId = item.ItemId,
                        PickedQuantity = item.QuantityToPick > 0 ? item.QuantityToPick : item.OrderedQuantity
                    }).ToList()
                };

                var result = await _apiService.PostAsync<OutboundViewModel>("outbound/pick", pickDto);
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Items picked successfully. Inventory has been reserved.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message ?? "Failed to pick items";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error picking outbound items");
                TempData["ErrorMessage"] = "Error picking items";
                return View(model);
            }
        }

        // GET: Outbound/Ship/5
        public async Task<IActionResult> Ship(Guid id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            try
            {
                var result = await _apiService.GetAsync<OutboundViewModel>($"outbound/{id}");
                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["ErrorMessage"] = "Outbound order not found";
                    return RedirectToAction(nameof(Index));
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading outbound for shipping");
                TempData["ErrorMessage"] = "Error loading outbound order";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Outbound/Ship/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ship(Guid id, ShipOutboundViewModel model)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            try
            {
                var shipDto = new ShipOutboundDto
                {
                    OutboundId = model.Id
                };

                var result = await _apiService.PostAsync<OutboundViewModel>("outbound/ship", shipDto);
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Outbound order shipped successfully. Inventory has been deducted.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message ?? "Failed to ship outbound order";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shipping outbound order");
                TempData["ErrorMessage"] = "Error shipping outbound order";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            try
            {
                var result = await _apiService.PostAsync<OutboundViewModel>($"outbound/{id}/cancel", null);
                if (result.IsSuccess)
                    TempData["SuccessMessage"] = "Outbound order cancelled successfully";
                else
                    TempData["ErrorMessage"] = "Failed to cancel outbound order";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling outbound order");
                TempData["ErrorMessage"] = "Error cancelling outbound order";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        // Helper method
        private async Task LoadProductsAndLocations()
        {
            try
            {
                var productsResult = await _apiService.GetAsync<PagedResult<ProductViewModel>>("products?pageSize=1000&status=active");
                ViewBag.Products = productsResult.Data?.Items ?? new List<ProductViewModel>();

                var locationsResult = await _apiService.GetAsync<PagedResult<LocationViewModel>>("locations?pageSize=1000&isActive=true");
                ViewBag.Locations = locationsResult.Data?.Items ?? new List<LocationViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products and locations");
                ViewBag.Products = new List<ProductViewModel>();
                ViewBag.Locations = new List<LocationViewModel>();
            }
        }
    }
}
