using System.Collections.Generic;

namespace _1131740.Models
{
    public class OrderLog
    {
        public string OrderNo { get; set; } = "";
        public string OrderDate { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
        public int TotalAmount { get; set; }
        public string Status { get; set; } = "";

        public string Username { get; set; } = "";

        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}