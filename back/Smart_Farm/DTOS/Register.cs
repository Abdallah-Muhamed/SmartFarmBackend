namespace Smart_Farm.DTOS;

public class RegisterDTO
{
    public required string First_name { get; set; }

    public required string Last_name { get; set; }

    public required string Email { get; set; }

    public required string Address_line { get; set; }

    public required string City_name { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public required string Role { get; set; }

    public required string Password { get; set; }

    public required string Phone { get; set; }
}
