using System.Text.Json.Serialization;
using Dockyra.Domain.Enums;
using Dockyra.Domain.ValueObjects;

namespace Dockyra.Domain.Entities;

public sealed class ApplicationEntry
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string DisplayName { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string? Arguments { get; set; }
    public string? ParentFolderId { get; set; }
    public int SortOrder { get; set; }
    public bool IsPinned { get; set; } = true;

    [JsonIgnore]
    public byte[]? IconBgra32 { get; set; }

    [JsonIgnore]
    public bool IsRunning { get; set; }

    [JsonIgnore]
    public bool IsActive { get; set; }

    [JsonIgnore]
    public ulong? RunningWindowHandle { get; set; }

    [JsonIgnore]
    public int? RunningProcessId { get; set; }
}

public sealed class ApplicationGroup
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = IconReference.DefaultFolder.GlyphOrPath;
    public string ColorHex { get; set; } = HexColor.DefaultBlue.Value;
    public string Description { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public FolderViewMode ViewMode { get; set; } = FolderViewMode.Grid;
    public List<ApplicationEntry> Applications { get; set; } = [];
    public List<FolderItem> SubFolders { get; set; } = [];

    [JsonIgnore]
    public IReadOnlyList<ApplicationEntry> PreviewApplications => Applications.Take(4).ToList();

    [JsonIgnore]
    public bool HasRunningApplications => Applications.Any(a => a.IsRunning);

    [JsonIgnore]
    public bool HasActiveApplication => Applications.Any(a => a.IsActive);
}

