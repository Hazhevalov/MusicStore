using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Models
{
    public class Sales
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customers Customer { get; set; } = null!;

        public DateOnly SaleDate { get; set; }
        public decimal TotalAmount { get; set; }

        public List<SaleItems> SaleItems { get; set; } = new();
    }
}
