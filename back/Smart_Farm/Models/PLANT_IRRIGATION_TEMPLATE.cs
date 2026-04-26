using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm.Models;

[Table("PLANT_IRRIGATION_TEMPLATE")]
public partial class PLANT_IRRIGATION_TEMPLATE
{
    [Key]
    public int PTid { get; set; }

    public int? PSid { get; set; }

    public int? Pid { get; set; }

    [StringLength(100)]
    public string Irrigation_name { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Water_amount { get; set; }

    public int? Frequency_value { get; set; }

    [StringLength(20)]
    public string Frequency_unit { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Description { get; set; }

    [ForeignKey("PSid")]
    [InverseProperty("PLANT_IRRIGATION_TEMPLATEs")]
    public virtual PLANT_STAGE PSidNavigation { get; set; }

    [ForeignKey("Pid")]
    [InverseProperty("PLANT_IRRIGATION_TEMPLATEs")]
    public virtual PLANT PidNavigation { get; set; }

    [InverseProperty("PTidNavigation")]
    public virtual ICollection<IRRIGATION> IRRIGATIONs { get; set; } = new List<IRRIGATION>();
}