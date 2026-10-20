using Microsoft.EntityFrameworkCore;
using _1131740.Models;

namespace _1131740.Data
{
    public class CmsContext : DbContext
    {
        public CmsContext(DbContextOptions<CmsContext> options) : base(options)
        {
        }

        public DbSet<Member> Members { get; set; }
        public DbSet<Product> Products { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Member>().ToTable("TableMembers1131740");
            modelBuilder.Entity<Product>().ToTable("TableProducts1131740");
        }
    }
}