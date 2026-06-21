using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class SectionItem : BaseEntity
{
    public Guid SectionId { get; set; }
    public SectionItemType ItemType { get; set; }
    public Guid ItemId { get; set; }
    public int Position { get; set; }
    public bool IsPreviewAllowed { get; set; }
    public bool IsMandatory { get; set; } = true;
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableUntil { get; set; }

    public Section Section { get; set; } = null!;
    public Video? Video { get; set; }
    public Quiz? Quiz { get; set; }
    public Document? Document { get; set; }
    public LiveSession? LiveSession { get; set; }
}
