using Dockyra.Domain.Enums;

namespace Dockyra.Domain.Entities;

public sealed class FolderItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string GroupId { get; set; } = string.Empty;
    public string? ParentFolderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public FolderViewMode ViewMode { get; set; } = FolderViewMode.Grid;
    public List<FolderItem> Children { get; set; } = [];
    public List<ApplicationEntry> Items { get; set; } = [];
}

