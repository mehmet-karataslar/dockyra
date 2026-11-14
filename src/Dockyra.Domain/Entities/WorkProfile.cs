namespace Dockyra.Domain.Entities;

public sealed class WorkProfile
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LayoutId { get; set; } = "default-layout";
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public List<string> EnabledGroupIds { get; set; } = [];

    public static IReadOnlyList<WorkProfile> CreateBuiltInProfiles() =>
    [
        new WorkProfile
        {
            Id = "profile-dev",
            Name = "Yazılım Geliştirme",
            Description = "IDE, terminal, tarayıcı ve Git araçları odaklı çalışma alanı",
            LayoutId = "default-layout",
            IsDefault = true,
            SortOrder = 0,
            EnabledGroupIds = ["grp-dev", "grp-browsers", "grp-comm"]
        },
        new WorkProfile
        {
            Id = "profile-daily",
            Name = "Günlük Kullanım",
            Description = "Tarayıcı, iletişim ve dosya yönetimi odaklı profil",
            LayoutId = "default-layout",
            IsDefault = false,
            SortOrder = 1,
            EnabledGroupIds = ["grp-browsers", "grp-comm", "grp-system"]
        },
        new WorkProfile
        {
            Id = "profile-gaming",
            Name = "Oyun & Performans",
            Description = "Oyun başlatıcıları, iletişim ve düşük kaynak tüketimi",
            LayoutId = "default-layout",
            IsDefault = false,
            SortOrder = 2,
            EnabledGroupIds = ["grp-comm", "grp-system"]
        }
    ];
}

