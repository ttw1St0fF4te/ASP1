using Microsoft.EntityFrameworkCore;
using PR1.Models;

namespace PR1.Data;

public class ProductContext : DbContext
{
    public ProductContext(DbContextOptions<ProductContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}