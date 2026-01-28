using System.ComponentModel.DataAnnotations;

namespace WMS.Web.Models
{
    public class InventoryListViewModel
    {
        public List<InventoryViewModel> Inventories { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public string? SearchTerm { get; set; }
        public string? FilterLocation { get; set; }
    }

    public class InventoryViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Product is required")]
        public Guid ProductId { get; set; }

        public string ProductSKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        public Guid LocationId { get; set; }

        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;

        // Match API DTO properties
        [Required(ErrorMessage = "Quantity on hand is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Quantity must be 0 or greater")]
        public decimal QuantityOnHand { get; set; }
        public decimal QuantityReserved { get; set; }
        public decimal QuantityAvailable { get; set; }

        // Keep Quantity as alias for backward compatibility
        public decimal Quantity => QuantityOnHand;

        public decimal? ReorderLevel { get; set; }
        public decimal? MaxStockLevel { get; set; }
        public string UOM { get; set; } = string.Empty;
        public DateTime LastStockDate { get; set; }
        public DateTime LastUpdated => LastStockDate;
        public string? LastUpdatedBy { get; set; }
    }

    public class InventoryTransactionListViewModel
    {
        public List<InventoryTransactionViewModel> Transactions { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public string? FilterType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class InventoryTransactionViewModel
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public string TransactionNumber { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? ReferenceType { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}
