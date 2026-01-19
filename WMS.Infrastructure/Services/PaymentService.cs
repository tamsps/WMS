using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WMS.Application.Common.Models;
using WMS.Application.DTOs.Payment;
using WMS.Application.Interfaces;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Infrastructure.Data;

namespace WMS.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly WMSDbContext _context;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        WMSDbContext context,
        IRepository<Payment> paymentRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaymentDto>> GetByIdAsync(Guid id)
    {
        var payment = await _context.Payments
            .Include(p => p.Outbound)
            .Include(p => p.PaymentEvents)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null)
        {
            return Result<PaymentDto>.Failure("Payment not found");
        }

        return Result<PaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<PagedResult<PaymentDto>>> GetAllAsync(int pageNumber, int pageSize, string? status = null)
    {
        var query = _context.Payments
            .Include(p => p.Outbound)
            .Include(p => p.PaymentEvents)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, out var paymentStatus))
        {
            query = query.Where(p => p.Status == paymentStatus);
        }

        var totalCount = await query.CountAsync();
        var payments = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<PaymentDto>
        {
            Items = payments.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Result<PagedResult<PaymentDto>>.Success(result);
    }

    public async Task<Result<PaymentDto>> CreateAsync(CreatePaymentDto dto, string currentUser)
    {
        // Validate outbound exists if provided
        if (dto.OutboundId.HasValue)
        {
            var outbound = await _context.Outbounds.FindAsync(dto.OutboundId.Value);
            if (outbound == null)
            {
                return Result<PaymentDto>.Failure("Outbound not found");
            }

            // Check if payment already exists for this outbound
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OutboundId == dto.OutboundId.Value);
            
            if (existingPayment != null)
            {
                return Result<PaymentDto>.Failure("Payment already exists for this outbound");
            }
        }

        if (!Enum.TryParse<PaymentType>(dto.PaymentType, out var paymentType))
        {
            return Result<PaymentDto>.Failure("Invalid payment type");
        }

        var payment = new Payment
        {
            OutboundId = dto.OutboundId,
            PaymentNumber = await GeneratePaymentNumberAsync(),
            PaymentType = paymentType,
            Status = PaymentStatus.Pending,
            Amount = dto.Amount,
            Currency = dto.Currency ?? "USD",
            PaymentMethod = dto.PaymentMethod,
            CreatedBy = currentUser
        };

        // Add initial payment event
        payment.PaymentEvents.Add(new PaymentEvent
        {
            EventType = "Created",
            EventData = JsonSerializer.Serialize(new { Amount = dto.Amount, Currency = dto.Currency }),
            Notes = "Payment created",
            CreatedBy = currentUser
        });

        await _paymentRepository.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        var result = await GetByIdAsync(payment.Id);
        return Result<PaymentDto>.Success(result.Data!, "Payment created successfully");
    }

    public async Task<Result<PaymentDto>> InitiateAsync(InitiatePaymentDto dto, string currentUser)
    {
        var payment = await _context.Payments
            .Include(p => p.PaymentEvents)
            .FirstOrDefaultAsync(p => p.Id == dto.PaymentId);

        if (payment == null)
        {
            return Result<PaymentDto>.Failure("Payment not found");
        }

        if (payment.Status == PaymentStatus.Confirmed)
        {
            return Result<PaymentDto>.Failure("Payment is already confirmed");
        }

        if (payment.Status == PaymentStatus.Cancelled)
        {
            return Result<PaymentDto>.Failure("Cannot initiate cancelled payment");
        }

        payment.PaymentGateway = dto.PaymentGateway;
        payment.UpdatedBy = currentUser;
        payment.UpdatedAt = DateTime.UtcNow;

        payment.PaymentEvents.Add(new PaymentEvent
        {
            EventType = "Initiated",
            EventData = JsonSerializer.Serialize(new { Gateway = dto.PaymentGateway }),
            Notes = $"Payment initiated via {dto.PaymentGateway}",
            CreatedBy = currentUser
        });

        await _unitOfWork.SaveChangesAsync();

        var result = await GetByIdAsync(payment.Id);
        return Result<PaymentDto>.Success(result.Data!, "Payment initiated successfully");
    }

    public async Task<Result<PaymentDto>> ConfirmAsync(ConfirmPaymentDto dto, string currentUser)
    {
        var payment = await _context.Payments
            .Include(p => p.PaymentEvents)
            .FirstOrDefaultAsync(p => p.Id == dto.PaymentId);

        if (payment == null)
        {
            return Result<PaymentDto>.Failure("Payment not found");
        }

        if (payment.Status == PaymentStatus.Confirmed)
        {
            return Result<PaymentDto>.Failure("Payment is already confirmed");
        }

        if (payment.Status == PaymentStatus.Cancelled)
        {
            return Result<PaymentDto>.Failure("Cannot confirm cancelled payment");
        }

        payment.Status = PaymentStatus.Confirmed;
        payment.PaymentDate = DateTime.UtcNow;
        payment.ConfirmedDate = DateTime.UtcNow;
        payment.ExternalPaymentId = dto.ExternalPaymentId;
        payment.TransactionReference = dto.TransactionReference;
        payment.UpdatedBy = currentUser;
        payment.UpdatedAt = DateTime.UtcNow;

        payment.PaymentEvents.Add(new PaymentEvent
        {
            EventType = "Confirmed",
            EventData = JsonSerializer.Serialize(new 
            { 
                ExternalPaymentId = dto.ExternalPaymentId, 
                TransactionReference = dto.TransactionReference 
            }),
            Notes = "Payment confirmed",
            CreatedBy = currentUser
        });

        await _unitOfWork.SaveChangesAsync();

        var result = await GetByIdAsync(payment.Id);
        return Result<PaymentDto>.Success(result.Data!, "Payment confirmed successfully");
    }

    public async Task<Result> ProcessWebhookAsync(PaymentWebhookDto dto)
    {
        var payment = await _context.Payments
            .Include(p => p.PaymentEvents)
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == dto.ExternalPaymentId);

        if (payment == null)
        {
            return Result.Failure("Payment not found with specified external payment ID");
        }

        if (!Enum.TryParse<PaymentStatus>(dto.Status, out var status))
        {
            status = payment.Status; // Keep current status if parsing fails
        }

        payment.Status = status;
        payment.UpdatedAt = DateTime.UtcNow;

        if (status == PaymentStatus.Confirmed)
        {
            payment.PaymentDate = DateTime.UtcNow;
            payment.ConfirmedDate = DateTime.UtcNow;
        }

        payment.PaymentEvents.Add(new PaymentEvent
        {
            EventType = "WebhookReceived",
            EventData = dto.EventData,
            Notes = $"Webhook received: {dto.Status}",
            CreatedBy = "System"
        });

        await _unitOfWork.SaveChangesAsync();

        return Result.Success("Webhook processed successfully");
    }

    public async Task<Result<bool>> CanShipAsync(Guid outboundId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.OutboundId == outboundId);

        if (payment == null)
        {
            // No payment required, can ship
            return Result<bool>.Success(true);
        }

        // For COD and Postpaid, shipping is allowed
        if (payment.PaymentType == PaymentType.COD || payment.PaymentType == PaymentType.Postpaid)
        {
            return Result<bool>.Success(true);
        }

        // For Prepaid, payment must be confirmed
        bool canShip = payment.Status == PaymentStatus.Confirmed;
        
        if (!canShip)
        {
            return Result<bool>.Failure($"Payment must be confirmed before shipping. Current status: {payment.Status}");
        }

        return Result<bool>.Success(true);
    }

    private async Task<string> GeneratePaymentNumberAsync()
    {
        var today = DateTime.UtcNow;
        var prefix = $"PAY-{today:yyyyMMdd}";
        
        var lastPayment = await _context.Payments
            .Where(p => p.PaymentNumber.StartsWith(prefix))
            .OrderByDescending(p => p.PaymentNumber)
            .FirstOrDefaultAsync();

        if (lastPayment == null)
        {
            return $"{prefix}-0001";
        }

        var lastNumber = int.Parse(lastPayment.PaymentNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D4}";
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            OutboundId = payment.OutboundId,
            OutboundNumber = payment.Outbound?.OutboundNumber,
            PaymentNumber = payment.PaymentNumber,
            PaymentType = payment.PaymentType.ToString(),
            Status = payment.Status.ToString(),
            Amount = payment.Amount,
            Currency = payment.Currency,
            ExternalPaymentId = payment.ExternalPaymentId,
            PaymentGateway = payment.PaymentGateway,
            PaymentMethod = payment.PaymentMethod,
            PaymentDate = payment.PaymentDate,
            ConfirmedDate = payment.ConfirmedDate,
            TransactionReference = payment.TransactionReference,
            CreatedAt = payment.CreatedAt
        };
    }
}
