using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm.Models;

[Table("PRODUCT_IMAGE")]
public class PRODUCT_IMAGE
{
    [Key]
    public int Id { get; set; }

    public int Pid { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Url { get; set; }

    public int SortOrder { get; set; }

    [ForeignKey("Pid")]
    public PRODUCT? PidNavigation { get; set; }
}

