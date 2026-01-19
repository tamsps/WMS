using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IApiService apiService, ILogger<PaymentController> logger)
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
                var queryString = $"api/payment?pageNumber={pageNumber}&pageSize={pageSize}";
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                if (!string.IsNullOrWhiteSpace(filterStatus))
                    queryString += $"&status={Uri.EscapeDataString(filterStatus)}";

                var result = await _apiService.GetAsync<ApiResponse<PagedResult<PaymentViewModel>>>(queryString);

                var viewModel = new PaymentListViewModel
                {
                    Items = result?.Data?.Items ?? new List<PaymentViewModel>(),
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
                _logger.LogError(ex, "Error loading payments");
                TempData["ErrorMessage"] = "Error loading payments";
                return View(new PaymentListViewModel());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            try
            {
                var result = await _apiService.GetAsync<ApiResponse<PaymentViewModel>>($"api/payment/{id}");
                if (result?.Data == null)
                {
                    TempData["ErrorMessage"] = "Payment not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment details");
                TempData["ErrorMessage"] = "Error loading payment details";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");
            return View(new CreatePaymentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePaymentViewModel model)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _apiService.PostAsync<ApiResponse<PaymentViewModel>>("api/payment", model);
                if (result?.IsSuccess == true)
                {
                    TempData["SuccessMessage"] = "Payment created successfully";
                    return RedirectToAction(nameof(Details), new { id = result.Data?.Id });
                }
                TempData["ErrorMessage"] = result?.Message ?? "Failed to create payment";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                TempData["ErrorMessage"] = "Error creating payment";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
                return RedirectToAction("Login", "Account");

            try
            {
                var result = await _apiService.PostAsync<ApiResponse<PaymentViewModel>>($"api/payment/{id}/confirm", null);
                if (result?.IsSuccess == true)
                    TempData["SuccessMessage"] = "Payment confirmed successfully";
                else
                    TempData["ErrorMessage"] = "Failed to confirm payment";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment");
                TempData["ErrorMessage"] = "Error confirming payment";
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
