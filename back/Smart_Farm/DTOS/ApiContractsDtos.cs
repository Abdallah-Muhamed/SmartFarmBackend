    namespace Smart_Farm.DTOS;

    public class CropRequestDto
    {
        public int? Pid { get; set; }
        public int? FarmId { get; set; }
        public string? Notes { get; set; }
        public decimal? Area_size { get; set; }
        public DateOnly? Start_date { get; set; }
        public string? Current_Stage { get; set; }
    }

    public class CropResponseDto
    {
        public int Cid { get; set; }
        public int? Pid { get; set; }
        public int? FarmId { get; set; }
        public string? Notes { get; set; }
        public decimal? Area_size { get; set; }
        public DateOnly? Start_date { get; set; }
        public string? Soil_type { get; set; }
        public string? Current_Stage { get; set; }
        public int? Uid { get; set; }
    }

    public class ProductRequestDto
    {
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public DateOnly? Added_date { get; set; }
        public int? Quantity { get; set; }
        public int? Cid { get; set; }
        public string? Category { get; set; }
        public double? Rating { get; set; }
    }

    public class ProductResponseDto
    {
        public int Pid { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public DateOnly? Added_date { get; set; }
        public int? Quantity { get; set; }
        public int? Uid { get; set; }
        public int? Cid { get; set; }
        public string? Category { get; set; }

        public double? Rating { get; set; }
    }

    public class OrderRequestDto
    {
        public string? Status { get; set; }
        public DateOnly? Order_date { get; set; }
        public int? Quantity { get; set; }
        public decimal? Total_price { get; set; }
        public int? Pid { get; set; }
        public string? Payment_method { get; set; }
        public string? Promo_code { get; set; }
        public decimal? Discount_amount { get; set; }
        public string? Order_notes { get; set; }
    }

    public class BatchOrderLineDto
    {
        public string? Status { get; set; }
        public DateOnly? Order_date { get; set; }
        public int? Quantity { get; set; }
        public decimal? Total_price { get; set; }
        public int? Pid { get; set; }
    }

    public class BatchOrderRequestDto
    {
        public List<BatchOrderLineDto>? Items { get; set; }
        public string? Payment_method { get; set; }
        public string? Promo_code { get; set; }
        public decimal? Discount_amount { get; set; }
        public string? Order_notes { get; set; }
    }

    public class TaskRequestDto
    {
        public DateOnly? Date { get; set; }
        public string? Label { get; set; }
        public string? Content { get; set; }
        public string? State { get; set; }
    }

    public class TaskResponseDto
    {
        public int Task_id { get; set; }
        public DateOnly? Date { get; set; }
        public string? Label { get; set; }
        public string? Content { get; set; }
        public string? State { get; set; }
        public int? Uid { get; set; }
    }

    public class UserUpdateDto
    {
        public string? First_name { get; set; }
        public string? Last_name { get; set; }
        public string? Email { get; set; }
        public string? Address_line { get; set; }
        public string? City_name { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Role { get; set; }
        public required List<string> Phones { get; set; }

    }

    public class IrrigationRequestDto
    {
        public string? Irrigation_name { get; set; }
        public string? Description { get; set; }
        public string? Frequency_unit { get; set; }
        public int? Frequency_value { get; set; }
        public decimal? Water_amount { get; set; }
        public int? Sis { get; set; }
        public int? Cid { get; set; }
    }

    public class IrrigationStageRequestDto
    {
        public string? Name_stage { get; set; }
        public int? Stage_order { get; set; }
        public string? Description { get; set; }
        public int? Cid { get; set; }
    }

    public class IrrigationRecommendationResponseDto
    {
        public int CropId { get; set; }
        public string? CurrentStage { get; set; }
        public int IntervalDays { get; set; }
        public string? NextIrrigationDate { get; set; }
        public string? LastIrrigationDate { get; set; }
        public bool IsDueToday { get; set; }
        public bool IsDoneToday { get; set; }
        public decimal RecommendedLiters { get; set; }
        public string? IrrigationMethod { get; set; }
        public int DurationMinutes { get; set; }
    }
public class StageIrrigationRecommendationDto
{
    public int? StageOrder { get; set; }
    public string? StageName { get; set; }
    public string? Description { get; set; }
    public int DurationDays { get; set; }
    public int StageStartDay { get; set; }
    public int StageEndDay { get; set; }
    public bool IsCurrentStage { get; set; }
    public int IntervalDays { get; set; }
    public string? IrrigationMethod { get; set; }
    public decimal RecommendedLiters { get; set; }
    public int DurationMinutes { get; set; }
}

public class AllStagesIrrigationResponseDto
{
    public int CropId { get; set; }
    public string? PlantingDate { get; set; }
    public int TotalDays { get; set; }
    public List<StageIrrigationRecommendationDto> Stages { get; set; } = new();
}