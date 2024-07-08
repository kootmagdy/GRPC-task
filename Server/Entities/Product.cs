namespace InventoryService.Models
{
    public enum Category
    {
     
        Food,
        Models,
        LapTops
    }
}
    public class ProductModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public double price { get; set; }
        public int quantity { get; set; }
        public Category category { get; set; }
        public DateTime expireDate { get; set; }
    }
   
}
