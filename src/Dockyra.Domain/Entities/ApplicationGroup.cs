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
}

