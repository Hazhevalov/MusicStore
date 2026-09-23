using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Models
{
    public class Customers
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal TotalSpent { get; set; }
        public DateOnly RegisteredAt { get; set; }

        public List<Reservations> Reservations { get; set; } = new();
        public List<Sales> Sales { get; set; } = new();
    }
}
