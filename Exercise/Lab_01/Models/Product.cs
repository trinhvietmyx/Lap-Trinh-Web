namespace Lab_01.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Price { get; set; }
        public string? OldPrice { get; set; }
        public string? Sale { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }
        
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
