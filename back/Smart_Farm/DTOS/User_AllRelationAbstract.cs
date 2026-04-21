namespace Smart_Farm.DTOS;

public class UserDto
{
    public int Uid { get; set; }
    public required string First_name { get; set; }
    public required string Last_name { get; set; }
    public required string Email { get; set; }
    public required string Address_line { get; set; }
    public required string City_name { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public required string Role { get; set; }

    public required List<string> Phones { get; set; }
}

public class CropDto
{
    public int CropId { get; set; }
    public required string CropName { get; set; }
}

public class ProductDto
{
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal Price { get; set; }
}

public class OrderDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
}

public class TaskDto
{
    public int TaskId { get; set; }
    public required string TaskName { get; set; }
}
