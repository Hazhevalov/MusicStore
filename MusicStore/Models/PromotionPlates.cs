using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Models
{
    public class PromotionPlates
    {
        public int PromotionId { get; set; }
        public Promotions Promotion { get; set; } = null!;
        public int PlateId { get; set; }
        public Plates Plate { get; set; } = null!;
    }
}
