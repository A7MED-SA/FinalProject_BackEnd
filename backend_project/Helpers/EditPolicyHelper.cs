using backend_project.DTOs.EditRequest;
using backend_project.Models;

namespace backend_project.Helpers;

/// <summary>
/// Static policy engine that determines whether a course edit requires admin approval
/// and assesses risk level based on edit type and context.
/// </summary>
public static class EditPolicyHelper
{
    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Course.Price),
        nameof(Course.Title),
        nameof(Course.CategoryId),
        nameof(Course.Level),
        nameof(Course.Language)
    };

    private static readonly HashSet<SectionItemType> CoreContentTypes = new()
    {
        SectionItemType.Video,
        SectionItemType.Quiz,
        SectionItemType.LiveSession
    };

    /// <summary>
    /// Determines whether an edit to a course requires admin approval.
    /// </summary>
    public static bool RequiresApproval(Course course, EditContext context)
    {
        // Edits before publishing never require approval
        if (course.Status != CourseStatus.Published)
            return false;

        // Delete operations always require approval
        if (context.Operation == EditOperation.Delete)
            return true;

        // Editing core educational content (Video, Quiz, LiveSession)
        if (context.ItemType.HasValue && CoreContentTypes.Contains(context.ItemType.Value))
            return true;

        // Editing sensitive course properties
        if (!string.IsNullOrEmpty(context.PropertyName) &&
            SensitiveProperties.Contains(context.PropertyName))
        {
            // Exception: price change under 10% is auto-approved
            if (context.PropertyName == nameof(Course.Price) &&
                context.OldValue is decimal oldPrice &&
                context.NewValue is decimal newPrice && oldPrice > 0)
            {
                var changePercent = Math.Abs(newPrice - oldPrice) / oldPrice;
                if (changePercent < 0.10m)
                    return false;
            }

            return true;
        }

        // Safe properties: description, slug edits are auto-approved
        if (context.PropertyName is nameof(Course.Description) or nameof(Course.Slug))
            return false;

        // Default: require approval for safety
        return true;
    }

    /// <summary>
    /// Assesses the risk level of an edit operation.
    /// </summary>
    public static EditRiskLevel AssessRisk(EditContext context)
    {
        if (context.Operation == EditOperation.Delete)
            return EditRiskLevel.High;

        if (context.ItemType.HasValue && CoreContentTypes.Contains(context.ItemType.Value))
            return EditRiskLevel.High;

        if (context.PropertyName == nameof(Course.Price))
        {
            if (context.OldValue is decimal oldP && context.NewValue is decimal newP && oldP > 0)
            {
                var change = Math.Abs(newP - oldP) / oldP;
                if (change > 0.50m) return EditRiskLevel.Critical;
                if (change > 0.20m) return EditRiskLevel.High;
            }
            return EditRiskLevel.Medium;
        }

        if (context.PropertyName is nameof(Course.Title) or nameof(Course.CategoryId))
            return EditRiskLevel.Medium;

        return EditRiskLevel.Low;
    }

    /// <summary>
    /// Determines the expiration period for a pending edit request.
    /// </summary>
    public static TimeSpan GetExpirationPeriod(EditRequestType type, EditOperation operation)
    {
        if (operation == EditOperation.Delete)
            return TimeSpan.FromDays(3);

        return TimeSpan.FromDays(7);
    }
}

/// <summary>
/// Context object carrying all information needed for the edit policy engine to evaluate.
/// </summary>
public class EditContext
{
    public EditRequestType TargetType { get; set; }
    public EditOperation Operation { get; set; }
    public string? PropertyName { get; set; }
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public Guid? TargetEntityId { get; set; }
    public SectionItemType? ItemType { get; set; }
}
