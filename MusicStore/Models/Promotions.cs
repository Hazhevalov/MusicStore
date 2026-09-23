using System;
using System.Collections.Generic;
using System.Text;

namespace Exam.Models
{
    public class Promotions
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int GenreId { get; set; }
        public Genres Genre { get; set; } = null!;  

        public decimal DiscountPercent { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public List<PromotionPlates> PromotionPlates { get; set; } = new();
    }
}
