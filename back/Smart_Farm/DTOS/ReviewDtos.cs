namespace Smart_Farm.DTOS;

public class ReviewRequestDto
{
    public int? Pid { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
}

public class ReviewResponseDto
{
    public int Rid { get; set; }
    public int? Pid { get; set; }
    public int? Uid { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedUtc { get; set; }
    public string? ReviewerName { get; set; }
}

