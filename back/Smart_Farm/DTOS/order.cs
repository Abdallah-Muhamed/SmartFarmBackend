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

    public required string ProductName { get; set; }
}
