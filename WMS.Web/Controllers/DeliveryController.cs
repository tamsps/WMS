using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers
{
    public class DeliveryController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<DeliveryController> _logger;

        public DeliveryController(IApiService apiService, ILogger<DeliveryController> logger)
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
                var queryString = $"delivery?pageNumber={pageNumber}&pageSize={pageSize}";
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                if (!string.IsNullOrWhiteSpace(filterStatus))
                    queryString += $"&status={Uri.EscapeDataString(filterStatus)}";

                var result = await _apiService.GetAsync<ApiResponse<PagedResult<DeliveryViewModel>>>(queryString);

                var viewModel = new DeliveryListViewModel
                {
                    Items = result?.Data?.Items ?? new List<DeliveryViewModel>(),
                    TotalCount = result?.Data?.TotalCount ?? 0,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    FilterStatus = filterStatus
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading deliveries");
                TempData["ErrorMessage"] = "Error loading deliveries";
                return View(new DeliveryListViewModel());
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            try
            {
                var result = await _apiService.GetAsync<ApiResponse<DeliveryViewModel>>($"delivery/{id}");
                if (result?.Data == null)
                {
                    TempData["ErrorMessage"] = "Delivery not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delivery details");
                TempData["ErrorMessage"] = "Error loading delivery details";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");
            return View(new CreateDeliveryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDeliveryViewModel model)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _apiService.PostAsync<ApiResponse<DeliveryViewModel>>("delivery", model);
                if (result?.IsSuccess == true)
                {
                    TempData["SuccessMessage"] = "Delivery created successfully";
                    return RedirectToAction(nameof(Details), new { id = result.Data?.Id });
                }
                TempData["ErrorMessage"] = result?.Message ?? "Failed to create delivery";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating delivery");
                TempData["ErrorMessage"] = "Error creating delivery";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, string status)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            try
            {
                var result = await _apiService.PatchAsync<ApiResponse<DeliveryViewModel>>($"delivery/{id}/status", new { status });
                if (result?.IsSuccess == true)
                    TempData["SuccessMessage"] = "Delivery status updated successfully";
                else
                    TempData["ErrorMessage"] = "Failed to update delivery status";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery status");
                TempData["ErrorMessage"] = "Error updating delivery status";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Track(string trackingNumber)
        {
            try
            {
                var result = await _apiService.GetAsync<ApiResponse<DeliveryViewModel>>($"delivery/tracking/{trackingNumber}");
                if (result?.Data == null)
                {
                    TempData["ErrorMessage"] = "Delivery not found";
                    return View("TrackNotFound");
                }
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking delivery");
                TempData["ErrorMessage"] = "Error tracking delivery";
                return View("TrackNotFound");
            }
        }
    }
}
