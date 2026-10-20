using System;

namespace _1131740.Models
{
    public class CustomerProfile
    {
        public string Id { get; set; } = "";
        public string Username { get; set; } = "";
        public int Balance { get; set; }
        public int TotalConsumed { get; set; }
        public string CreatedAt { get; set; } = "";
    }
}