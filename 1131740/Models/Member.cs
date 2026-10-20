using System.ComponentModel.DataAnnotations;

namespace _1131740.Models
{
    public class Member
    {
        [Key]
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public int? Balance { get; set; }
        public int? TotalConsumed { get; set; }
    }
}
