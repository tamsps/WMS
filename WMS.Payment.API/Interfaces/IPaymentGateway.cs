namespace WMS.Payment.API.Interfaces;

/// <summary>
/// Payment Gateway Interface
/// Abstracts payment gateway-specific implementations
/// Allows pluggable gateway providers (Stripe, PayPal, Square, etc.)
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Gateway name (e.g., "Stripe", "PayPal", "Square")
    /// </summary>
    string GatewayName { get; }
    
    /// <summary>
    /// Initiate payment session with gateway
    /// Returns payment URL/token for customer redirect
    /// </summary>
    /// <param name="amount">Payment amount</param>
    /// <param name="currency">Currency code (USD, EUR, etc.)</param>
    /// <param name="referenceId">WMS payment reference ID</param>
    /// <param name="metadata">Additional metadata (customer info, order details)</param>
    /// <returns>Payment session with redirect URL and session ID</returns>
    Task<PaymentGatewayResponse> InitiatePaymentAsync(
        decimal amount, 
        string currency, 
        string referenceId, 
        Dictionary<string, string>? metadata = null);
    
    /// <summary>
    /// Verify payment status with gateway
    /// Used to confirm webhook authenticity and get current status
    /// </summary>
    /// <param name="externalPaymentId">Gateway transaction ID</param>
    /// <returns>Payment status verification result</returns>
    Task<PaymentVerificationResponse> VerifyPaymentAsync(string externalPaymentId);
    
    /// <summary>
    /// Verify webhook signature
    /// Prevents spoofed callbacks
    /// </summary>
    /// <param name="payload">Raw webhook payload</param>
    /// <param name="signature">Signature from webhook headers</param>
    /// <param name="secret">Webhook secret key</param>
    /// <returns>True if signature is valid</returns>
    bool VerifyWebhookSignature(string payload, string signature, string secret);
    
    /// <summary>
    /// Parse webhook payload to standardized format
    /// </summary>
    /// <param name="payload">Raw webhook payload</param>
    /// <returns>Parsed webhook data</returns>
    WebhookPayload ParseWebhook(string payload);
}

/// <summary>
/// Payment gateway response from initiation
/// </summary>
public class PaymentGatewayResponse
{
    /// <summary>
    /// Success indicator
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Payment session ID from gateway
    /// </summary>
    public string? SessionId { get; set; }
    
    /// <summary>
    /// External payment ID (transaction ID)
    /// </summary>
    public string? ExternalPaymentId { get; set; }
    
    /// <summary>
    /// Payment URL for customer redirect
    /// Customer completes payment on this URL
    /// </summary>
    public string? PaymentUrl { get; set; }
    
    /// <summary>
    /// QR code for mobile payments (optional)
    /// </summary>
    public string? QrCode { get; set; }
    
    /// <summary>
    /// Expiration time for payment session
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Error message if initiation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Payment verification response
/// </summary>
public class PaymentVerificationResponse
{
    public bool IsSuccess { get; set; }
    public string? ExternalPaymentId { get; set; }
    public string Status { get; set; } = string.Empty; // Confirmed, Failed, Pending
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Parsed webhook payload
/// </summary>
public class WebhookPayload
{
    public string GatewayEventId { get; set; } = string.Empty;
    public string ExternalPaymentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string RawData { get; set; } = string.Empty;
}
