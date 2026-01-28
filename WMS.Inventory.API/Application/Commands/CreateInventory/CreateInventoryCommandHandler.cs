using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Entities;
using WMS.Domain.Interfaces;
using WMS.Domain.Data;
using WMS.Inventory.API.Common.Models;
using WMS.Inventory.API.DTOs.Inventory;
using WMS.Inventory.API.Application.Mappers;

namespace WMS.Inventory.API.Application.Commands.CreateInventory;

public class CreateInventoryCommandHandler : IRequestHandler<CreateInventoryCommand, Result<InventoryDto>>
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInventoryCommandHandler(WMSDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InventoryDto>> Handle(CreateInventoryCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Check if inventory already exists for this product and location
            var existingInventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == request.Dto.ProductId && i.LocationId == request.Dto.LocationId, cancellationToken);

            if (existingInventory != null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result<InventoryDto>.Failure("Inventory already exists for this product and location. Use adjust inventory instead.");
            }

            // Validate product exists
            var product = await _context.Products.FindAsync(new object[] { request.Dto.ProductId }, cancellationToken);
            if (product == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result<InventoryDto>.Failure("Product not found");
            }

            // Validate location exists
            var location = await _context.Locations.FindAsync(new object[] { request.Dto.LocationId }, cancellationToken);
            if (location == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result<InventoryDto>.Failure("Location not found");
            }

            // Create inventory record
            var inventory = new WMS.Domain.Entities.Inventory
            {
                ProductId = request.Dto.ProductId,
                LocationId = request.Dto.LocationId,
                QuantityOnHand = request.Dto.QuantityOnHand,
                QuantityReserved = request.Dto.QuantityReserved,
                LastStockDate = DateTime.UtcNow,
                Notes = request.Dto.Notes,
                CreatedBy = request.CurrentUser,
                CreatedAt = DateTime.UtcNow
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync(cancellationToken);

            // Create inventory transaction record
            var transaction = new InventoryTransaction
            {
                TransactionNumber = await GenerateTransactionNumberAsync(cancellationToken),
                TransactionType = Domain.Enums.TransactionType.Inbound,
                ProductId = request.Dto.ProductId,
                LocationId = request.Dto.LocationId,
                Quantity = request.Dto.QuantityOnHand,
                BalanceBefore = 0,
                BalanceAfter = request.Dto.QuantityOnHand,
                ReferenceType = "Manual",
                Notes = $"Manual inventory creation. {request.Dto.Notes}",
                CreatedBy = request.CurrentUser,
                CreatedAt = DateTime.UtcNow
            };

            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync();

            // Reload with navigation properties for mapping
            var createdInventory = await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Location)
                .FirstAsync(i => i.Id == inventory.Id, cancellationToken);

            return Result<InventoryDto>.Success(InventoryMapper.MapToDto(createdInventory), "Inventory created successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result<InventoryDto>.Failure($"Failed to create inventory: {ex.Message}");
        }
    }

    private async Task<string> GenerateTransactionNumberAsync(CancellationToken cancellationToken)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        var transactionNumber = $"TXN-{timestamp}-{random}";

        // Ensure uniqueness
        while (await _context.InventoryTransactions.AnyAsync(t => t.TransactionNumber == transactionNumber, cancellationToken))
        {
            random = new Random().Next(1000, 9999);
            transactionNumber = $"TXN-{timestamp}-{random}";
        }

        return transactionNumber;
    }
}