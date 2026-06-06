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
        public int? Cid { get; set; }
    }

    public class TaskResponseDto
    {
        public int Task_id { get; set; }
        public DateOnly? Date { get; set; }
        public string? Label { get; set; }
        public string? Content { get; set; }
        public string? State { get; set; }
        public int? Uid { get; set; }
        public int? Cid { get; set; }
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

    public class IrrigationDayDto
    {
        public int Cid { get; set; }
        public DateOnly Date { get; set; }
        public string? PlantName { get; set; }
        public string? StageName { get; set; }
        public string? SoilType { get; set; }
        public decimal AreaFeddan { get; set; }

        public bool IsIrrigationDay { get; set; }
        public bool? WasApplied { get; set; }
        public DateTime? AppliedAt { get; set; }

        public decimal Recommended_m3_per_feddan { get; set; }
        public decimal Recommended_m3_field { get; set; }
        public decimal Recommended_Liters { get; set; }
        public decimal? Applied_Liters { get; set; }

        public decimal ET0_mm { get; set; }
        public decimal Kc { get; set; }
        public decimal ETc_mm { get; set; }
        public decimal EffRain_mm { get; set; }
        public decimal TAW_mm { get; set; }
        public decimal RAW_mm { get; set; }
        public decimal DeplStart_mm { get; set; }
        public decimal DeplAfterEt_mm { get; set; }
        public decimal DeplEnd_mm { get; set; }
        public decimal Recommended_mm { get; set; }
        public decimal? Applied_mm { get; set; }

        public IrrigationAdviceReportDto? Reasoning { get; set; }
    }

    public class IrrigationCalendarDayDto
    {
        public DateOnly Date { get; set; }
        public string? StageName { get; set; }
        public bool IsIrrigationDay { get; set; }
        public bool? WasApplied { get; set; }
        public decimal Recommended_Liters { get; set; }
        public decimal? Applied_Liters { get; set; }
        public decimal DeplEnd_mm { get; set; }
        public decimal ETc_mm { get; set; }
        public decimal EffRain_mm { get; set; }
    }

    public class RecordIrrigationRequestDto
    {
        public DateOnly Date { get; set; }
        public bool Applied { get; set; }
        public decimal? Liters { get; set; }
    }

public class PlantStageDto
{
    public int PSid { get; set; }
    public int? Pid { get; set; }
    public string? Name_stage { get; set; }
    public int? Stage_order { get; set; }
    public int Duration_days { get; set; }
    public string? Description { get; set; }
}