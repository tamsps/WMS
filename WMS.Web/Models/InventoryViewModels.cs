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
        public Guid ProductId { get; set; }
        public string ProductSKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public Guid LocationId { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal? ReorderLevel { get; set; }
        public decimal? MaxStockLevel { get; set; }
        public string UOM { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
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
        public string ProductSKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? ReferenceType { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? Notes { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}
