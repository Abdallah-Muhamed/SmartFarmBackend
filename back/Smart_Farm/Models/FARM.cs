using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm.Models;

[Table("FARM")]
public partial class FARM
{
    [Key]
    public int FarmId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [Column(TypeName = "decimal(10, 6)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(10, 6)")]
    public decimal? Longitude { get; set; }

    [StringLength(100)]
    public string? Governorate { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(200)]
    public string? Address_line { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Area_size { get; set; }

    [StringLength(100)]
    public string? Default_Soil_type { get; set; }

    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Notes { get; set; }

    public int Uid { get; set; }

    [ForeignKey("Uid")]
    [InverseProperty("FARMs")]
    public virtual USER? UidNavigation { get; set; }

    [InverseProperty("FarmNavigation")]
    public virtual ICollection<CROP> CROPs { get; set; } = new List<CROP>();
}
