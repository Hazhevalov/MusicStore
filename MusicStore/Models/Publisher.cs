using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Models
{
    public class Publisher
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public List<Plates> Plates { get; set; } = new();
    }
}
