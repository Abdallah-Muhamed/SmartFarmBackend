using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Smart_Farm.Models;

[Table("REVIEW")]
public partial class REVIEW
{
    [Key]
    public int Rid { get; set; }

    public int? Pid { get; set; }

    public int? Uid { get; set; }

    public int? Rating { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Comment { get; set; }

    public DateTime CreatedUtc { get; set; }

    [ForeignKey("Pid")]
    public virtual PRODUCT? PidNavigation { get; set; }

    [ForeignKey("Uid")]
    public virtual USER? UidNavigation { get; set; }
}

