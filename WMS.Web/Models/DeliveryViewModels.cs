using System.ComponentModel.DataAnnotations;

namespace WMS.Web.Models
{
    public class DeliveryListViewModel
    {
        public List<DeliveryViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public string? SearchTerm { get; set; }
        public string? FilterStatus { get; set; }
    }

    public class DeliveryViewModel
    {
        public Guid Id { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public Guid OutboundId { get; set; }
        public string OutboundReference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public string? ShippingAddress { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class CreateDeliveryViewModel
    {
        [Required]
        [StringLength(50)]
        public string TrackingNumber { get; set; } = string.Empty;

        [Required]
        public Guid OutboundId { get; set; }

        [Required]
        [StringLength(100)]
        public string Carrier { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string RecipientPhone { get; set; } = string.Empty;

        public DateTime? EstimatedDeliveryDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
