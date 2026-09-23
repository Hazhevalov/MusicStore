using MusicStore.Data;
using MusicStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Data;

namespace MusicStore.Services;

public class StockService
{
    private readonly InventoryService _inventoryService;
    private readonly AuthorizationService _authorization;

    // Initializes stock operations and related services.
    public StockService(
        InventoryService inventoryService,
        AuthorizationService authorization)
    {
        _inventoryService = inventoryService;
        _authorization = authorization;
    }

    // Returns all active records with reference data.
    public List<Plates> GetAll()
    {
        using var db = new ApplicationContext();

        return db.Plates.Where(p => !p.IsArchived)
        .Include(p => p.Artist)
        .Include(p => p.Publisher)
        .Include(p => p.Genre)
        .AsNoTracking()
        .ToList();
    }
    // Returns an active record by ID.
    public Plates GetById(int id)
    {
        using var db = new ApplicationContext();

        return db.Plates.AsNoTracking().FirstOrDefault(p => p.Id == id && !p.IsArchived)
            ?? throw new ArgumentException("Record not found."); ;
    }

    // Receives units into stock and records the movement.
    public void Reciept(int plateId, int reciept, string reason)
    {
        _authorization.Require(UserRoles.Administrator, UserRoles.Manager);
        if (reciept <= 0)
            throw new ArgumentException("Receipt quantity must be greater than zero.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Specify a reason for the receipt.");
        if (reason.Trim().Length > 500)
            throw new ArgumentException("Reason cannot exceed 500 characters.");

        using var db = new ApplicationContext();
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);

        Plates plate = db.Plates.FirstOrDefault(p => p.Id == plateId && !p.IsArchived)
            ?? throw new ArgumentException("Record not found.");
        plate.Quantity += reciept;

        StockMovement stockMovement = new()
        {
            PlateId = plateId,
            QuantityChange = reciept,
            MovementType = StockMovementType.Receipt,
            CreatedAt = DateOnly.FromDateTime(DateTime.Today),
            Reason = reason.Trim()
        };
        db.StockMovements.Add(stockMovement);
        _inventoryService.SaveChanges(db);
        transaction.Commit();
    }
    // Writes units off stock after checking the available quantity.
    public void WriteOff(int plateId, int whiteOff, string reason)
    {
        _authorization.Require(UserRoles.Administrator, UserRoles.Manager);
        if (whiteOff <= 0)
            throw new ArgumentException("Write-off quantity must be greater than zero.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Specify a reason for the write-off.");
        if (reason.Trim().Length > 500)
            throw new ArgumentException("Reason cannot exceed 500 characters.");

        using var db = new ApplicationContext();
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        _inventoryService.ExpireReservations(db, today);

        var plate = db.Plates.FirstOrDefault(p => p.Id == plateId && !p.IsArchived)
            ?? throw new ArgumentException("Record not found.");

        int available = _inventoryService.GetAvailableQuantity(db, plate, today);
        if (available < whiteOff)
            throw new ArgumentException(
                $"Insufficient available stock. Available: {available} units.");

        plate.Quantity -= whiteOff;

        StockMovement stockMovement = new()
        {
            PlateId = plateId,
            QuantityChange = -whiteOff,
            MovementType = StockMovementType.WriteOff,
            CreatedAt = today,
            Reason = reason.Trim()
        };
        db.StockMovements.Add(stockMovement);
        _inventoryService.SaveChanges(db);
        transaction.Commit();
    }

    // Returns the stock movement history.
    public List<StockMovement> GetStockMovement()
    {
        using var db = new ApplicationContext();
        var movements = db.StockMovements
            .Include(p => p.Plate)
            .AsNoTracking()
            .ToList();

        return movements;
    }
}
