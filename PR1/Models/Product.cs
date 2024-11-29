namespace PR1.Models;

public class Product
{
    public int Id { get; set; }
    public string Category { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}

public class ProductList
{
    public List<Product> Products { get; set; }
}