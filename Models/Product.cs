namespace ShopEasy.Models
{
    public class Product
    {
        public int Id { get; set; }
        public String Name { get; set; } = String.Empty;
        public String? Description { get; set; }
        public decimal Price { get; set; }
        public String? ImageUrl { get; set; }
        public int Stock {  get; set; }

    }
}
