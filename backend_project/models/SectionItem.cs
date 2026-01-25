using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("section_items")]
public class SectionItem : BaseEntity
{

    [Required]
    [Column("section_id")]
    public Guid SectionId { get; set; }

    [Column("item_type")]
    [MaxLength(20)]
    public SectionItemType ItemType { get; set; }

    [Required]
    [Column("item_id")]
    public Guid ItemId { get; set; }

    [Column("position")]
    public int Position { get; set; } = 0;

    [Column("is_preview_allowed")]
    public bool IsPreviewAllowed { get; set; } = false;

    [Column("is_mandatory")]
    public bool IsMandatory { get; set; } = true;

    [Column("available_from")]
    public DateTime? AvailableFrom { get; set; }

    [Column("available_until")]
    public DateTime? AvailableUntil { get; set; }

    // Navigation Properties
    [ForeignKey("SectionId")]
    public virtual Section Section { get; set; } = null!;


}

public enum SectionItemType
{
    Video,
    Quiz,
    Document,
    LiveSession
}
