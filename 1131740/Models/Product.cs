using System.ComponentModel.DataAnnotations;

namespace _1131740.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }
        public string? ProductName { get; set; }
        public int Price { get; set; } 
        public int Stock { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Tag { get; set; }
        public string? ImageUrl { get; set; }

        public string? AllPlansData { get; set; }
    }
}