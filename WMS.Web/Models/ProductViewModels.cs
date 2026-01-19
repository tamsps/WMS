using System.ComponentModel.DataAnnotations;

namespace WMS.Web.Models;

// Product List View Model
public class ProductListViewModel
{
    public List<ProductViewModel> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
}

// Individual Product View Model
public class ProductViewModel
{
    public Guid Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string UOM { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public string? Barcode { get; set; }
    public string? Category { get; set; }
    public decimal? ReorderLevel { get; set; }
    public decimal? MaxStockLevel { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

// Create Product View Model
public class CreateProductViewModel
{
    [Required(ErrorMessage = "SKU is required")]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "UOM is required")]
    [Display(Name = "Unit of Measure")]
    public string UOM { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Weight must be positive")]
    public decimal Weight { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Length must be positive")]
    public decimal Length { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Width must be positive")]
    public decimal Width { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Height must be positive")]
    public decimal Height { get; set; }

    [StringLength(100)]
    public string? Barcode { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [Display(Name = "Reorder Level")]
    [Range(0, double.MaxValue, ErrorMessage = "Reorder level must be positive")]
    public decimal? ReorderLevel { get; set; }

    [Display(Name = "Max Stock Level")]
    [Range(0, double.MaxValue, ErrorMessage = "Max stock level must be positive")]
    public decimal? MaxStockLevel { get; set; }
}

// Edit Product View Model
public class EditProductViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "SKU is required")]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "UOM is required")]
    [Display(Name = "Unit of Measure")]
    public string UOM { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Weight must be positive")]
    public decimal Weight { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Length must be positive")]
    public decimal Length { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Width must be positive")]
    public decimal Width { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Height must be positive")]
    public decimal Height { get; set; }

    [StringLength(100)]
    public string? Barcode { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [Display(Name = "Reorder Level")]
    [Range(0, double.MaxValue, ErrorMessage = "Reorder level must be positive")]
    public decimal? ReorderLevel { get; set; }

    [Display(Name = "Max Stock Level")]
    [Range(0, double.MaxValue, ErrorMessage = "Max stock level must be positive")]
    public decimal? MaxStockLevel { get; set; }
}
