using System;

namespace _1131740.Models
{
    public class CartItem
    {
        public string CartItemId { get; set; } = Guid.NewGuid().ToString();
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public string SelectedPlan { get; set; }
        public int UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int SubTotal => UnitPrice * Quantity;
        public string RealName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string GameNickname { get; set; }
        public string? PlayerUID { get; set; }
        public string? GameAccount { get; set; }
        public string? GamePassword { get; set; }
    }
}