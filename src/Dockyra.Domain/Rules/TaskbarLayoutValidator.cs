using Dockyra.Domain.Entities;

namespace Dockyra.Domain.Rules;

public static class TaskbarLayoutValidator
{
    public const int MinIconSize = 24;
    public const int MaxIconSize = 64;
    public const int MinBarHeight = 36;
    public const int MaxBarHeight = 96;

    public static bool Validate(TaskbarLayout layout, out IReadOnlyList<string> errors)
    {
        ArgumentNullException.ThrowIfNull(layout);
        List<string> issues = [];

        if (string.IsNullOrWhiteSpace(layout.Id))
        {
            issues.Add("Layout Id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(layout.Name))
        {
            issues.Add("Layout Name cannot be empty.");
        }

        if (layout.IconSize is < MinIconSize or > MaxIconSize)
        {
            issues.Add($"IconSize must be between {MinIconSize} and {MaxIconSize}.");
        }

        if (layout.BarHeight is < MinBarHeight or > MaxBarHeight)
        {
            issues.Add($"BarHeight must be between {MinBarHeight} and {MaxBarHeight}.");
        }

        if (layout.Opacity is < 0.2 or > 1.0)
        {
            issues.Add("Opacity must be between 0.2 and 1.0.");
        }

        if (layout.Sections.Count == 0)
        {
            issues.Add("TaskbarLayout must contain at least one section.");
        }

        errors = issues;
        return issues.Count == 0;
    }
}

