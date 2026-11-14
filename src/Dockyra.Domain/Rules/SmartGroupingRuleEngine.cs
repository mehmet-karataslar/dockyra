using Dockyra.Domain.Entities;
using Dockyra.Domain.ValueObjects;

namespace Dockyra.Domain.Rules;

/// <summary>
/// Evaluates application classification rules. User-defined rules always take precedence over built-in automatic rules.
/// </summary>
public sealed class SmartGroupingRuleEngine
{
    private static readonly IReadOnlyList<ClassificationRule> BuiltInRules =
    [
        new ClassificationRule { Id = "builtin-vscode", Pattern = "Code", MatchField = RuleMatchField.ProcessName, TargetGroupId = "grp-dev", IsUserDefined = false, Priority = 10 },
        new ClassificationRule { Id = "builtin-devenv", Pattern = "devenv", MatchField = RuleMatchField.ProcessName, TargetGroupId = "grp-dev", IsUserDefined = false, Priority = 10 },
        new ClassificationRule { Id = "builtin-wt", Pattern = "WindowsTerminal", MatchField = RuleMatchField.ProcessName, TargetGroupId = "grp-dev", IsUserDefined = false, Priority = 10 },
        new ClassificationRule { Id = "builtin-pwsh", Pattern = "pwsh", MatchField = RuleMatchField.ProcessName, TargetGroupId = "grp-dev", IsUserDefined = false, Priority = 10 },
        new ClassificationRule { Id = "builtin-chrome", Pattern = "chrome", MatchField = RuleMatchField.ProcessName, TargetGroupId = "grp-browsers", IsUserDefined = false, Priority = 10 },
        new ClassificationRule { Id = "builtin-msedge", Pattern = "msedge", MatchField = RuleMatchField.ProcessName, TargetGroupId = "grp-browsers", IsUserDefined = false, Priority = 10 },
        new ClassificationRule { Id = "builtin-firefox", Pattern = "firefox", MatchField = RuleMatchField.ProcessName, TargetGroupId = "grp-browsers", IsUserDefined = false, Priority = 10 },
        new ClassificationRule { Id = "builtin-discord", Pattern = "Discord", MatchField = RuleMatchField.ProcessName, TargetGroupId = "grp-comm", IsUserDefined = false, Priority = 10 },
        new ClassificationRule { Id = "builtin-slack", Pattern = "slack", MatchField = RuleMatchField.ProcessName, TargetGroupId = "grp-comm", IsUserDefined = false, Priority = 10 },
        new ClassificationRule { Id = "builtin-teams", Pattern = "ms-teams", MatchField = RuleMatchField.ProcessName, TargetGroupId = "grp-comm", IsUserDefined = false, Priority = 10 },
        new ClassificationRule { Id = "builtin-explorer", Pattern = "explorer", MatchField = RuleMatchField.ProcessName, TargetGroupId = "grp-system", IsUserDefined = false, Priority = 10 }
    ];

    public string? SuggestGroupId(
        AppIdentity app,
        IEnumerable<ClassificationRule>? userRules = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        IEnumerable<ClassificationRule> orderedUserRules = (userRules ?? [])
            .Where(r => r.IsUserDefined && !string.IsNullOrWhiteSpace(r.Pattern))
            .OrderByDescending(r => r.Priority);

        foreach (ClassificationRule rule in orderedUserRules)
        {
            if (IsMatch(app, rule))
            {
                return rule.TargetGroupId;
            }
        }

        foreach (ClassificationRule rule in BuiltInRules.OrderByDescending(r => r.Priority))
        {
            if (IsMatch(app, rule))
            {
                return rule.TargetGroupId;
            }
        }

        return null;
    }

    private static bool IsMatch(AppIdentity app, ClassificationRule rule)
    {
        string? candidate = rule.MatchField switch
        {
            RuleMatchField.ProcessName => app.ProcessName,
            RuleMatchField.ExecutablePath => app.ExecutablePath,
            RuleMatchField.WindowTitle => app.WindowTitle,
            _ => app.ProcessName
        };

        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        return candidate.Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase);
    }
}

