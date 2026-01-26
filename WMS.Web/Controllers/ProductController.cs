using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

public class ProductController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IApiService apiService, ILogger<ProductController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    // GET: Product
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20, string? searchTerm = null, string? status = null)
    {
        if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var queryString = $"products?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(searchTerm))
                queryString += $"&searchTerm={searchTerm}";
            if (!string.IsNullOrEmpty(status))
                queryString += $"&status={status}";

            var result = await _apiService.GetAsync<ApiResponse<PagedResult<ProductViewModel>>>(queryString);

            var model = new ProductListViewModel
            {
                Products = result?.Data?.Items ?? new List<ProductViewModel>(),
                TotalCount = result?.Data?.TotalCount ?? 0,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                Status = status
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products");
            TempData["ErrorMessage"] = "Error loading products";
            return View(new ProductListViewModel());
        }
    }

    // GET: Product/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var result = await _apiService.GetAsync<ApiResponse<ProductViewModel>>($"products/{id}");
            
            if (result?.Data == null)
            {
                TempData["ErrorMessage"] = "Product not found";
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product details");
            TempData["ErrorMessage"] = "Error loading product details";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Product/Create
    public IActionResult Create()
    {
        if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }

    // POST: Product/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductViewModel model)
    {
        if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _apiService.PostAsync<ApiResponse<ProductViewModel>>("products", model);

            if (result?.IsSuccess == true)
            {
                TempData["SuccessMessage"] = "Product created successfully";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result?.Message ?? "Failed to create product");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            ModelState.AddModelError(string.Empty, "Error creating product");
            return View(model);
        }
    }

    // GET: Product/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var result = await _apiService.GetAsync<ApiResponse<ProductViewModel>>($"products/{id}");
            
            if (result?.Data == null)
            {
                TempData["ErrorMessage"] = "Product not found";
                return RedirectToAction(nameof(Index));
            }

            var model = new EditProductViewModel
            {
                Id = result.Data.Id,
                SKU = result.Data.SKU,
                Name = result.Data.Name,
                Description = result.Data.Description,
                UOM = result.Data.UOM,
                Weight = result.Data.Weight,
                Length = result.Data.Length,
                Width = result.Data.Width,
                Height = result.Data.Height,
                Barcode = result.Data.Barcode,
                Category = result.Data.Category,
                ReorderLevel = result.Data.ReorderLevel,
                MaxStockLevel = result.Data.MaxStockLevel
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product for edit");
            TempData["ErrorMessage"] = "Error loading product";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Product/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditProductViewModel model)
    {
        if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
        {
            return RedirectToAction("Login", "Account");
        }

        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _apiService.PutAsync<ApiResponse<ProductViewModel>>($"products/{id}", model);

            if (result?.IsSuccess == true)
            {
                TempData["SuccessMessage"] = "Product updated successfully";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result?.Message ?? "Failed to update product");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product");
            ModelState.AddModelError(string.Empty, "Error updating product");
            return View(model);
        }
    }

    // POST: Product/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var success = await _apiService.DeleteAsync($"products/{id}");

            if (success)
            {
                TempData["SuccessMessage"] = "Product deleted successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete product";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product");
            TempData["ErrorMessage"] = "Error deleting product";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Product/Activate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(Guid id)
    {
        if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var result = await _apiService.PatchAsync<ApiResponse<ProductViewModel>>($"products/{id}/activate");

            if (result?.IsSuccess == true)
            {
                TempData["SuccessMessage"] = "Product activated successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to activate product";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating product");
            TempData["ErrorMessage"] = "Error activating product";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Product/Deactivate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var result = await _apiService.PatchAsync<ApiResponse<ProductViewModel>>($"products/{id}/deactivate");

            if (result?.IsSuccess == true)
            {
                TempData["SuccessMessage"] = "Product deactivated successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to deactivate product";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating product");
            TempData["ErrorMessage"] = "Error deactivating product";
            return RedirectToAction(nameof(Index));
        }
    }
}

// Helper class for paged results
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
