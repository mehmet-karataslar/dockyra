namespace Dockyra.Domain.Entities;

public enum RuleMatchField
{
    ProcessName = 0,
    ExecutablePath = 1,
    WindowTitle = 2
}

public sealed class ClassificationRule
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Pattern { get; set; } = string.Empty;
    public RuleMatchField MatchField { get; set; } = RuleMatchField.ProcessName;
    public string TargetGroupId { get; set; } = string.Empty;
    public bool IsUserDefined { get; set; }
    public int Priority { get; set; }
}

