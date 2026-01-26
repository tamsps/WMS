using System.ComponentModel.DataAnnotations;

namespace WMS.Web.Models
{
    public class LocationListViewModel
    {
        public List<LocationViewModel> Locations { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public string? SearchTerm { get; set; }
        public string? FilterStatus { get; set; }
        public string? FilterType { get; set; }
    }

    public class LocationViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Zone { get; set; }
        public string? Aisle { get; set; }
        public string? Rack { get; set; }
        public string? Shelf { get; set; }
        public string? Bin { get; set; }
        public decimal Capacity { get; set; }
        public decimal OccupiedCapacity { get; set; }
        public Guid? ParentLocationId { get; set; }
        public string? ParentLocationName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class CreateLocationViewModel
    {
        [Required(ErrorMessage = "Location code is required")]
        [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location type is required")]
        public string Type { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Zone { get; set; }

        [StringLength(50)]
        public string? Aisle { get; set; }

        [StringLength(50)]
        public string? Rack { get; set; }

        [StringLength(50)]
        public string? Shelf { get; set; }

        [StringLength(50)]
        public string? Bin { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Capacity must be greater than or equal to 0")]
        public decimal Capacity { get; set; }

        public Guid? ParentLocationId { get; set; }
    }

    public class EditLocationViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Location code is required")]
        [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location type is required")]
        public string Type { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Zone { get; set; }

        [StringLength(50)]
        public string? Aisle { get; set; }

        [StringLength(50)]
        public string? Rack { get; set; }

        [StringLength(50)]
        public string? Shelf { get; set; }

        [StringLength(50)]
        public string? Bin { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Capacity must be greater than or equal to 0")]
        public decimal Capacity { get; set; }

        public Guid? ParentLocationId { get; set; }
    }
}
