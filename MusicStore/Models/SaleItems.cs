using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Models
{
    public class SaleItems
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public Sales Sale { get; set; } = null!;

        public int PlateId { get; set; }
        public Plates Plate { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitPrise { get; set; }
        public decimal DiscountPercent { get; set; }

    }
    public enum StatisticsPeriod
    {
        Day = 1,
        Week = 2,
        Month = 3,
        Year = 4
    }
}
