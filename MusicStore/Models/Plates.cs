using System;
using System.Collections.Generic;
using System.Text;

namespace Exam.Models
{
    public class Plates
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int ArtistId { get; set; }
        public Artists Artist { get; set; } = null!;

        public int PublisherId { get; set; }
        public Publisher Publisher { get; set; } = null!;

        public int GenreId { get; set; }
        public Genres Genre { get; set; } = null!;

        public int TrackCount { get; set; }
        public int ReleaseYear { get; set; }
        public decimal CostPrise { get; set; }
        public decimal SalePrise { get; set; }
        public int Quantity { get; set; }
        public bool IsArchived { get; set; }
        public byte[] RowVersion { get; set; } = null!;

        public List<StockMovement> StockMovements { get; set; } = new();
        public List<PromotionPlates> PromotionPlates { get; set; } = new();
        public List<Reservations> Reservations { get; set; } = new();
        public List<SaleItems> SaleItems { get; set; } = new();

    }
}
