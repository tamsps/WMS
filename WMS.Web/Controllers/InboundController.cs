using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers
{
    public class InboundController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<InboundController> _logger;

        public InboundController(IApiService apiService, ILogger<InboundController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Inbound
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20, string? searchTerm = null, string? filterStatus = null)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var queryString = $"inbound?pageNumber={pageNumber}&pageSize={pageSize}";
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                if (!string.IsNullOrWhiteSpace(filterStatus))
                    queryString += $"&status={Uri.EscapeDataString(filterStatus)}";

                var result = await _apiService.GetAsync<PagedResult<InboundViewModel>>(queryString);

                var viewModel = new InboundListViewModel
                {
                    Items = result.IsSuccess ? result.Data?.Items ?? new List<InboundViewModel>() : new List<InboundViewModel>(),
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
                _logger.LogError(ex, "Error loading inbounds");
                TempData["ErrorMessage"] = "Error loading inbound orders";
                return View(new InboundListViewModel());
            }
        }

        // GET: Inbound/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _apiService.GetAsync<InboundViewModel>($"inbound/{id}");

                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors ?? new List<string>());
                    return RedirectToAction(nameof(Index));
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inbound details");
                TempData["ErrorMessage"] = "Error loading inbound order details";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Inbound/Create
        public async Task<IActionResult> Create()
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            await LoadProductsAndLocations();
            return View(new CreateInboundViewModel());
        }

        // POST: Inbound/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInboundViewModel model)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                await LoadProductsAndLocations();
                return View(model);
            }

            try
            {
                var result = await _apiService.PostAsync<InboundViewModel>("inbound", model);

                if (result.IsSuccess && result.Data != null)
                {
                    _logger.LogInformation("Inbound order created successfully with ID: {InboundId}", result.Data.Id);
                    TempData["SuccessMessage"] = "Inbound order created successfully. Please receive the items.";
                    
                    // Redirect to Receive view so user can manually receive the goods
                    return RedirectToAction(nameof(Receive), new { id = result.Data.Id });
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors ?? new List<string>());
                    await LoadProductsAndLocations();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inbound order");
                TempData["ErrorMessage"] = "Error creating inbound order";
                await LoadProductsAndLocations();
                return View(model);
            }
        }

        // GET: Inbound/Receive/5
        public async Task<IActionResult> Receive(Guid id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _apiService.GetAsync<InboundViewModel>($"inbound/{id}");

                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors ?? new List<string>());
                    return RedirectToAction(nameof(Index));
                }

                var receiveModel = new ReceiveInboundViewModel
                {
                    Id = result.Data.Id,
                    ReferenceNumber = result.Data.ReferenceNumber,
                    SupplierName = result.Data.SupplierName,
                    Status = result.Data.Status,
                    Items = result.Data.Items.Select(i => new ReceiveInboundItemViewModel
                    {
                        ItemId = i.Id,
                        ProductId = i.ProductId,
                        ProductSku = i.ProductSku,
                        ProductName = i.ProductName,
                        LocationId = i.LocationId,
                        LocationCode = i.LocationCode,
                        LocationName = i.LocationName,
                        ExpectedQuantity = i.ExpectedQuantity,
                        ReceivedQuantity = i.ReceivedQuantity,
                        UOM = i.UOM,
                        Notes = i.Notes
                    }).ToList()
                };

                return View(receiveModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inbound for receiving");
                TempData["ErrorMessage"] = "Error loading inbound order";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Inbound/Receive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(Guid id, ReceiveInboundViewModel model)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Transform the view model to the correct DTO structure expected by the API
                var receiveDto = new ReceiveInboundDto
                {
                    InboundId = model.Id,
                    Items = model.Items.Select(item => new ReceiveInboundItemDto
                    {
                        InboundItemId = item.ItemId,  // Map ItemId to InboundItemId
                        ReceivedQuantity = item.QuantityToReceive > 0 ? item.QuantityToReceive : item.ExpectedQuantity,
                        DamagedQuantity = 0,  // Can be enhanced to allow user input
                        Notes = item.Notes
                    }).ToList()
                }; 

                var result = await _apiService.PostAsync<InboundViewModel>($"inbound/{id}/receive", receiveDto);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Inbound order received successfully. Inventory has been updated.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors ?? new List<string>());
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving inbound order");
                TempData["ErrorMessage"] = "Error receiving inbound order";
                return View(model);
            }
        }

        // POST: Inbound/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(Guid id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _apiService.PostAsync<InboundViewModel>($"inbound/{id}/complete", null);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Inbound order completed successfully";
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors ?? new List<string>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing inbound order");
                TempData["ErrorMessage"] = "Error completing inbound order";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Inbound/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _apiService.PostAsync<InboundViewModel>($"inbound/{id}/cancel", null);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Inbound order cancelled successfully";
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors ?? new List<string>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling inbound order");
                TempData["ErrorMessage"] = "Error cancelling inbound order";
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
