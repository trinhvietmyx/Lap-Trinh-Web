using Microsoft.EntityFrameworkCore;
using Lab_01.Models;

namespace Lab_01.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Áo" },
                new Category { Id = 2, Name = "Quần" },
                new Category { Id = 3, Name = "Váy" }
            );

            // Seed 20 Products
            var products = new List<Product>();
            string[] names = { "Áo sơ mi trắng", "Áo thun polo", "Áo khoác gió", "Áo len cổ lọ", "Áo hoodie", 
                              "Quần tây công sở", "Quần jeans xanh", "Quần short kaki", "Quần jogger", "Quần vải ống rộng",
                              "Váy hoa nhí", "Váy lụa dài", "Váy công chúa", "Chân váy xếp ly", "Váy quây dạ hội",
                              "Áo vest nam", "Quần kaki đen", "Váy len mùa đông", "Áo thun unisex", "Quần nỉ thể thao" };
            
            string[] prices = { "350,000đ", "290,000đ", "550,000đ", "420,000đ", "480,000đ",
                               "450,000đ", "520,000đ", "280,000đ", "390,000đ", "410,000đ",
                               "580,000đ", "750,000đ", "1,200,000đ", "320,000đ", "1,500,000đ",
                               "2,500,000đ", "380,000đ", "600,000đ", "250,000đ", "340,000đ" };

            string[] slugs = { "ao-so-mi-trang", "ao-thun-polo", "ao-khoak-gio", "ao-len-co-lo", "ao-hoodie", 
                               "quan-tay-cong-so", "quan-jeans-xanh", "quan-short-kaki", "quan-jogger", "quan-vai-ong-rong",
                               "vay-hoa-nhi", "vay-lua-dai", "vay-cong-chua", "chan-vay-xep-ly", "vay-quay-da-hoi",
                               "ao-vest-nam", "quan-kaki-den", "vay-len-mua-dong", "ao-thun-unisex", "quan-ni-the-thao" };

            for (int i = 0; i < 20; i++)
            {
                int catId = i < 5 ? 1 : (i < 10 ? 2 : (i < 15 ? 3 : (i % 3 + 1))); 
                products.Add(new Product
                {
                    Id = i + 1,
                    Name = names[i],
                    Price = prices[i],
                    OldPrice = i % 2 == 0 ? "800,000đ" : null,
                    Sale = i % 2 == 0 ? "-30%" : null,
                    Image = $"/IMG/{slugs[i]}.jpg",
                    Description = "Sản phẩm thời trang thiết kế riêng biệt, chất liệu cao cấp mang lại sự thoải mái và tự tin cho người mặc.",
                    CategoryId = catId
                });
            }

            modelBuilder.Entity<Product>().HasData(products);
        }
    }
}
