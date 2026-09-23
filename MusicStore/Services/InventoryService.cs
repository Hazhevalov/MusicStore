using Exam.Data;
using Exam.Models;
using Microsoft.EntityFrameworkCore;

namespace Exam.Services;

public class InventoryService
{
    // Saves inventory changes and converts concurrency failures into user-friendly errors.
    public void SaveChanges(ApplicationContext db)
    {
        try
        {
            db.SaveChanges();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException(
                "The data was changed by another user. Please try again.", ex);
        }
    }

    // Returns the quantity currently held by active reservations.
    public int GetReservedQuantity(
        ApplicationContext db,
        int plateId,
        DateOnly today,
        int? excludedReservationId = null)
    {
        return db.Reservations
            .Where(r => r.PlatesId == plateId &&
                        r.Status == ReservationStatus.Active &&
                        r.ExpiresAt >= today &&
                        (!excludedReservationId.HasValue || r.Id != excludedReservationId.Value))
            .Sum(r => (int?)(r.Quantity - r.FulfilledQuantity)) ?? 0;
    }

    // Returns the physical stock that is not reserved.
    public int GetAvailableQuantity(
        ApplicationContext db,
        Plates plate,
        DateOnly today) =>
        plate.Quantity - GetReservedQuantity(db, plate.Id, today);

    // Marks overdue active reservations as expired.
    public int ExpireReservations(ApplicationContext db, DateOnly today)
    {
        return db.Reservations
            .Where(r => r.Status == ReservationStatus.Active && r.ExpiresAt < today)
            .ExecuteUpdate(setters => setters
                .SetProperty(r => r.Status, ReservationStatus.Expired));
    }
}
