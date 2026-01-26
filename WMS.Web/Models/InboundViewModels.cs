using System.ComponentModel.DataAnnotations;

namespace WMS.Web.Models
{
    public class InboundListViewModel
    {
        public List<InboundViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public string? SearchTerm { get; set; }
        public string? FilterStatus { get; set; }
    }

    public class InboundViewModel
    {
        public Guid Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ExpectedDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public int TotalItems { get; set; }
        public int ReceivedItems { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public List<InboundItemViewModel> Items { get; set; } = new();
    }

    public class InboundItemViewModel
    {
        public Guid Id { get; set; }
        public Guid InboundId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductSku { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public Guid LocationId { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public decimal ExpectedQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public string UOM { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class CreateInboundViewModel
    {
        [Required(ErrorMessage = "Reference number is required")]
        [StringLength(50)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Supplier name is required")]
        [StringLength(200)]
        public string SupplierName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expected date is required")]
        public DateTime ExpectedDate { get; set; } = DateTime.Now.AddDays(1);

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<CreateInboundItemViewModel> Items { get; set; } = new();
    }

    public class CreateInboundItemViewModel
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid LocationId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Expected quantity must be greater than 0")]
        public decimal ExpectedQuantity { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }
    }

    public class ReceiveInboundViewModel
    {
        public Guid Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<ReceiveInboundItemViewModel> Items { get; set; } = new();
    }

    public class ReceiveInboundItemViewModel
    {
        public Guid ItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSku { get; set; } = string.Empty;
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public decimal ExpectedQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal QuantityToReceive { get; set; }
        public string UOM { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    // DTOs for API communication
    public class ReceiveInboundDto
    {
        public Guid InboundId { get; set; }
        public List<ReceiveInboundItemDto> Items { get; set; } = new();
    }

    public class ReceiveInboundItemDto
    {
        public Guid InboundItemId { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal? DamagedQuantity { get; set; }
        public string? Notes { get; set; }
    }
}
