using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers
{
    public class LocationController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<LocationController> _logger;

        public LocationController(IApiService apiService, ILogger<LocationController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Location
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? filterStatus = null, string? filterType = null)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var queryString = $"locations?pageNumber={pageNumber}&pageSize={pageSize}";
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                if (!string.IsNullOrWhiteSpace(filterStatus))
                    queryString += $"&isActive={filterStatus == "active"}";
                if (!string.IsNullOrWhiteSpace(filterType))
                    queryString += $"&type={Uri.EscapeDataString(filterType)}";

                var result = await _apiService.GetAsync<PagedResult<LocationViewModel>>(queryString);

                var viewModel = new LocationListViewModel
                {
                    Locations = result.Data?.Items ?? new List<LocationViewModel>(),
                    TotalCount = result.Data?.TotalCount ?? 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    FilterStatus = filterStatus,
                    FilterType = filterType
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading locations");
                TempData["ErrorMessage"] = "Error loading locations";
                return View(new LocationListViewModel());
            }
        }

        // GET: Location/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _apiService.GetAsync<LocationViewModel>($"locations/{id}");

                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["ErrorMessage"] = "Location not found";
                    return RedirectToAction(nameof(Index));
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading location details");
                TempData["ErrorMessage"] = "Error loading location details";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Location/Create
        public async Task<IActionResult> Create()
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            await LoadParentLocations();
            return View();
        }

        // POST: Location/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLocationViewModel model)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                await LoadParentLocations();
                return View(model);
            }

            try
            {
                var result = await _apiService.PostAsync<LocationViewModel>("locations", model);

                if (result.IsSuccess && result.Data != null)
                {
                    TempData["SuccessMessage"] = "Location created successfully";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors ?? new List<string>());
                    await LoadParentLocations();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating location");
                TempData["ErrorMessage"] = "Error creating location";
                await LoadParentLocations();
                return View(model);
            }
        }

        // GET: Location/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _apiService.GetAsync<LocationViewModel>($"locations/{id}");

                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["ErrorMessage"] = "Location not found";
                    return RedirectToAction(nameof(Index));
                }

                var location = result.Data;
                var editModel = new EditLocationViewModel
                {
                    Id = location.Id,
                    Code = location.Code,
                    Name = location.Name,
                    Type = location.Type,
                    Zone = location.Zone,
                    Aisle = location.Aisle,
                    Rack = location.Rack,
                    Shelf = location.Shelf,
                    Bin = location.Bin,
                    Capacity = location.Capacity,
                    ParentLocationId = location.ParentLocationId
                };

                await LoadParentLocations(id);
                return View(editModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading location for edit");
                TempData["ErrorMessage"] = "Error loading location";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Location/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditLocationViewModel model)
        {
            if (string.IsNullOrEmpty(_apiService.GetAccessToken()))
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Invalid location ID";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await LoadParentLocations(id);
                return View(model);
            }

            try
            {
                var result = await _apiService.PutAsync<LocationViewModel>($"locations/{id}", model);

                if (result.IsSuccess && result.Data != null)
                {
                    TempData["SuccessMessage"] = "Location updated successfully";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors ?? new List<string>());
                    await LoadParentLocations(id);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location");
                TempData["ErrorMessage"] = "Error updating location";
                await LoadParentLocations(id);
                return View(model);
            }
        }

        // POST: Location/Delete/5
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
                var success = await _apiService.DeleteAsync($"locations/{id}");

                if (success)
                {
                    TempData["SuccessMessage"] = "Location deleted successfully";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete location";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting location");
                TempData["ErrorMessage"] = "Error deleting location";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Location/Activate/5
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
                var result = await _apiService.PatchAsync<LocationViewModel>($"locations/{id}/activate", null);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Location activated successfully";
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors ?? new List<string>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating location");
                TempData["ErrorMessage"] = "Error activating location";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Location/Deactivate/5
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
                var result = await _apiService.PatchAsync<LocationViewModel>($"locations/{id}/deactivate", null);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Location deactivated successfully";
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors ?? new List<string>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating location");
                TempData["ErrorMessage"] = "Error deactivating location";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // Helper method to load parent locations
        private async Task LoadParentLocations(Guid? excludeId = null)
        {
            try
            {
                var result = await _apiService.GetAsync<PagedResult<LocationViewModel>>("locations?pageSize=1000");
                var locations = result.Data?.Items ?? new List<LocationViewModel>();

                if (excludeId.HasValue)
                {
                    locations = locations.Where(l => l.Id != excludeId.Value).ToList();
                }

                ViewBag.ParentLocations = locations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading parent locations");
                ViewBag.ParentLocations = new List<LocationViewModel>();
            }
        }
    }
}
