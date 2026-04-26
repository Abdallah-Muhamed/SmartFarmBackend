using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm.Models;

[Table("PLANT_STAGE")]
public partial class PLANT_STAGE
{
    [Key]
    public int PSid { get; set; }

    public int? Pid { get; set; }

    [StringLength(100)]
    public string Name_stage { get; set; }

    public int? Stage_order { get; set; }

    public int Duration_days { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Description { get; set; }

    [ForeignKey("Pid")]
    [InverseProperty("PLANT_STAGEs")]
    public virtual PLANT PidNavigation { get; set; }

    [InverseProperty("PSidNavigation")]
    public virtual ICollection<PLANT_IRRIGATION_TEMPLATE> PLANT_IRRIGATION_TEMPLATEs { get; set; } = new List<PLANT_IRRIGATION_TEMPLATE>();

    [InverseProperty("PSidNavigation")]
    public virtual ICollection<IRRIGATION_STAGE> IRRIGATION_STAGEs { get; set; } = new List<IRRIGATION_STAGE>();
}