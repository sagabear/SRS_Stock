using SQLite;

namespace SRS_Stock.Core.Models;

public class Product
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string Name { get; set; } = string.Empty;

    [Indexed]
    public string Barcode { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal Quantity { get; set; }
}
