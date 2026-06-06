namespace Smart_Farm.DTOS;

public class OrderDTO
{
    public int Oid { get; set; }

    public required string Status { get; set; }

    public DateOnly? Order_date { get; set; }

    public int? Quantity { get; set; }

    public decimal? Total_price { get; set; }

    public int? Pid { get; set; }

    public int? Uid { get; set; }

    public required string UserName { get; set; }

    public string? BuyerName { get; set; }

    public required string ProductName { get; set; }

    // Seller contact info
    public string? SellerName { get; set; }

    public string? SellerPhone { get; set; }

    public string? SellerAddress { get; set; }

    public string? SellerCity { get; set; }

    // Buyer contact info
    public string? BuyerPhone { get; set; }

    public string? BuyerAddress { get; set; }

    public string? BuyerCity { get; set; }

    public string? Payment_method { get; set; }

    public string? Promo_code { get; set; }

    public decimal? Discount_amount { get; set; }

    public string? Order_notes { get; set; }
}
