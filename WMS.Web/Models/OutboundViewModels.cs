using System.ComponentModel.DataAnnotations;

namespace WMS.Web.Models
{
    public class OutboundListViewModel
    {
        public List<OutboundViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public string? SearchTerm { get; set; }
        public string? FilterStatus { get; set; }
    }

    public class OutboundViewModel
    {
        public Guid Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string OutboundNumber { get; set; } = string.Empty;  // Added
        public string? OrderNumber { get; set; }  // Added
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public string? TrackingNumber { get; set; }
        public int TotalItems { get; set; }
        public int PickedItems { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public List<OutboundItemViewModel> Items { get; set; } = new();
    }

    public class OutboundItemViewModel
    {
        public Guid Id { get; set; }
        public Guid OutboundId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductSku { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public Guid LocationId { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public decimal OrderedQuantity { get; set; }
        public decimal PickedQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }  // Added
        public string UOM { get; set; } = string.Empty;
    }

    public class CreateOutboundViewModel
    {
        [Required]
        [StringLength(50)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? ShippingAddress { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<CreateOutboundItemViewModel> Items { get; set; } = new();
    }

    public class CreateOutboundItemViewModel
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid LocationId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal OrderedQuantity { get; set; }
    }

    // DTOs for API communication
    public class PickOutboundDto
    {
        public Guid OutboundId { get; set; }
        public List<PickOutboundItemDto> Items { get; set; } = new();
    }

    public class PickOutboundItemDto
    {
        public Guid OutboundItemId { get; set; }
        public decimal PickedQuantity { get; set; }
    }

    public class ShipOutboundDto
    {
        public Guid OutboundId { get; set; }
    }

    // View Models for Pick process
    public class PickOutboundViewModel
    {
        public Guid Id { get; set; }
        public string OutboundNumber { get; set; } = string.Empty;
        public string? OrderNumber { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<PickOutboundItemViewModel> Items { get; set; } = new();
    }

    public class PickOutboundItemViewModel
    {
        public Guid ItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductSku { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public Guid LocationId { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public decimal OrderedQuantity { get; set; }
        public decimal PickedQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal QuantityToPick { get; set; }
        public string UOM { get; set; } = string.Empty;
    }

    // View Model for Ship process
    public class ShipOutboundViewModel
    {
        public Guid Id { get; set; }
        public string OutboundNumber { get; set; } = string.Empty;
        public string? OrderNumber { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<OutboundItemViewModel> Items { get; set; } = new();
    }
}
