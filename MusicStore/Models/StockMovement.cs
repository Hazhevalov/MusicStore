using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Models
{
    public class StockMovement
    {
        public int Id { get; set; }
        public int PlateId { get; set; }
        public Plates Plate { get; set; } = null!;

        public int QuantityChange { get; set; }
        public StockMovementType MovementType { get; set; }
        public DateOnly CreatedAt { get; set; }
        public string Reason { get; set; } = null!;
    }
    public enum StockMovementType
    {
        // Stock receipt.
        Receipt = 1,
        // Sale.
        Sale = 2,
        // Write-off.
        WriteOff = 3
    }
}
