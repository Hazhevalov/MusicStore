using System;
using System.Collections.Generic;
using System.Text;

namespace Exam.Models
{
    public class Reservations
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customers Customer { get; set; } = null!;

        public int PlatesId { get; set; }
        public Plates Plate { get; set; } = null!;

        public int Quantity { get; set; }
        public int FulfilledQuantity { get; set; }
        public DateOnly ReservedAt { get; set; }
        public DateOnly ExpiresAt { get; set; }
        public ReservationStatus Status { get; set; }
    }

    public enum ReservationStatus
    {
        Active = 1,
        Completed = 2,
        Cancelled = 3,
        Expired = 4
    }
}
