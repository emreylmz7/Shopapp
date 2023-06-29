using Microsoft.EntityFrameworkCore;
using shopapp.data.Configurations;
using shopapp.entity;

namespace shopapp.data.Concrete.EfCore
{
   public class ShopContext:DbContext
   {
      public ShopContext(DbContextOptions options): base(options)
      {
         
         
      }
      

      
      public DbSet<Product> Products { get; set; }
      public DbSet<Category> Categories { get; set; }
      public DbSet<Card> Cards { get; set; }
      public DbSet<CardItem> CardItems { get; set; }
      public DbSet<Order> Orders { get; set; }
      public DbSet<OrderItem> OrderItems { get; set; }


      // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
      // {
      //    optionsBuilder.UseMySQL(@"server=localhost;port=3306;Database=y_shopappdb;user=root;password=mysql123;");
      // }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         modelBuilder.ApplyConfiguration(new ProductConfiguration());
         modelBuilder.ApplyConfiguration(new CategoryConfiguration());
         modelBuilder.ApplyConfiguration(new ProductCategoryConfiguration());

         modelBuilder.Seed();
      }

      
   }
}