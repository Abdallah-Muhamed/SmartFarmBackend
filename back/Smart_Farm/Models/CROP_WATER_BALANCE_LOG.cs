using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm.Models;

[Table("CROP_WATER_BALANCE_LOG")]
public partial class CROP_WATER_BALANCE_LOG
{
    [Key]
    public int Id { get; set; }

    public int Cid { get; set; }

    public DateOnly Date { get; set; }

    // FAO reference ET0 (mm/day).
    [Column(TypeName = "decimal(6, 2)")]
    public decimal? ET0_mm { get; set; }

    [Column(TypeName = "decimal(4, 2)")]
    public decimal? Kc { get; set; }

    // Actual crop evapotranspiration for the day (mm).
    [Column(TypeName = "decimal(6, 2)")]
    public decimal? ETc_mm { get; set; }

    // Effective rainfall after runoff/capping (mm).
    [Column(TypeName = "decimal(6, 2)")]
    public decimal? EffRain_mm { get; set; }

    // Irrigation applied this day (mm).
    [Column(TypeName = "decimal(6, 2)")]
    public decimal? Irrig_mm { get; set; }

    // Depletion at start of day (mm).
    [Column(TypeName = "decimal(8, 2)")]
    public decimal? DeplStart_mm { get; set; }

    // Depletion at end of day (mm, after ETc, rain, and irrigation).
    [Column(TypeName = "decimal(8, 2)")]
    public decimal? DeplEnd_mm { get; set; }

    // TAW for this stage (mm).
    [Column(TypeName = "decimal(8, 2)")]
    public decimal? TAW_mm { get; set; }

    // RAW = p × TAW (mm).
    [Column(TypeName = "decimal(8, 2)")]
    public decimal? RAW_mm { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Note { get; set; }

    [ForeignKey("Cid")]
    [InverseProperty("WaterBalanceLogs")]
    public virtual CROP CidNavigation { get; set; }
}
