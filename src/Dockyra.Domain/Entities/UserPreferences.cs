using Dockyra.Domain.Enums;

namespace Dockyra.Domain.Entities;

public sealed class UserPreferences
{
    public ThemeKind Theme { get; set; } = ThemeKind.Dark;
    public string LanguageCode { get; set; } = "tr-TR";
    public string ActiveProfileId { get; set; } = "profile-dev";
    public bool AutoPowerSaveOnBattery { get; set; } = true;
    public string PreferredPowerMode { get; set; } = "Balanced";
    public bool LaunchAtWindowsStartup { get; set; }
    public bool EnableAnimations { get; set; } = true;
    public bool FirstRunCompleted { get; set; }
}

