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
                var queryString = $"api/outbound?pageNumber={pageNumber}&pageSize={pageSize}";
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                if (!string.IsNullOrWhiteSpace(filterStatus))
                    queryString += $"&status={Uri.EscapeDataString(filterStatus)}";

                var result = await _apiService.GetAsync<ApiResponse<PagedResult<OutboundViewModel>>>(queryString);

                var viewModel = new OutboundListViewModel
                {
                    Items = result?.Data?.Items ?? new List<OutboundViewModel>(),
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
                _logger.LogError(ex, "Error loading outbounds");
                TempData["ErrorMessage"] = "Error loading outbound orders";
                return View(new OutboundListViewModel());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            try
            {
                var result = await _apiService.GetAsync<ApiResponse<OutboundViewModel>>($"api/outbound/{id}");
                if (result?.Data == null)
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

        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");
            return View(new CreateOutboundViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOutboundViewModel model)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _apiService.PostAsync<ApiResponse<OutboundViewModel>>("api/outbound", model);
                if (result?.IsSuccess == true)
                {
                    TempData["SuccessMessage"] = "Outbound order created successfully";
                    return RedirectToAction(nameof(Details), new { id = result.Data?.Id });
                }
                TempData["ErrorMessage"] = result?.Message ?? "Failed to create outbound order";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating outbound order");
                TempData["ErrorMessage"] = "Error creating outbound order";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ship(int id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            try
            {
                var result = await _apiService.PostAsync<ApiResponse<OutboundViewModel>>($"api/outbound/{id}/ship", null);
                if (result?.IsSuccess == true)
                    TempData["SuccessMessage"] = "Outbound order shipped successfully";
                else
                    TempData["ErrorMessage"] = "Failed to ship outbound order";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shipping outbound order");
                TempData["ErrorMessage"] = "Error shipping outbound order";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            try
            {
                var result = await _apiService.PostAsync<ApiResponse<OutboundViewModel>>($"api/outbound/{id}/cancel", null);
                if (result?.IsSuccess == true)
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
    }
}
