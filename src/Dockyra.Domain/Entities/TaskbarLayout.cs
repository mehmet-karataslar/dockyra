using Dockyra.Domain.Enums;

namespace Dockyra.Domain.Entities;

public sealed class TaskbarSection
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public SectionKind Kind { get; set; } = SectionKind.RunningApplications;
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsPinned { get; set; } = true;
}

public sealed class TaskbarLayout
{
    public string Id { get; init; } = "default-layout";
    public string Name { get; set; } = "Default Layout";
    public TaskbarDockEdge DockEdge { get; set; } = TaskbarDockEdge.Bottom;
    public TaskbarAlignment Alignment { get; set; } = TaskbarAlignment.Center;
    public AutoHideMode AutoHide { get; set; } = AutoHideMode.OnFullscreenApp;
    public int IconSize { get; set; } = 36;
    public int BarHeight { get; set; } = 52;
    public int CornerRadius { get; set; } = 12;
    public int GroupSpacing { get; set; } = 8;
    public double Opacity { get; set; } = 0.94;
    public bool IsLocked { get; set; }
    public List<TaskbarSection> Sections { get; set; } = [];

    public static TaskbarLayout CreateDefault() =>
        new()
        {
            Id = "default-layout",
            Name = "Windows 11 Modular Default",
            DockEdge = TaskbarDockEdge.Bottom,
            Alignment = TaskbarAlignment.Center,
            AutoHide = AutoHideMode.OnFullscreenApp,
            IconSize = 36,
            BarHeight = 52,
            CornerRadius = 12,
            GroupSpacing = 8,
            Opacity = 0.94,
            IsLocked = false,
            Sections =
            [
                new TaskbarSection
                {
                    Id = "sec-groups",
                    Name = "Groups & Folders",
                    Kind = SectionKind.StartAndGroups,
                    SortOrder = 0,
                    IsVisible = true,
                    IsPinned = true
                },
                new TaskbarSection
                {
                    Id = "sec-running",
                    Name = "Open Applications",
                    Kind = SectionKind.RunningApplications,
                    SortOrder = 1,
                    IsVisible = true,
                    IsPinned = true
                },
                new TaskbarSection
                {
                    Id = "sec-system",
                    Name = "System & Power",
                    Kind = SectionKind.SystemStatus,
                    SortOrder = 2,
                    IsVisible = true,
                    IsPinned = true
                }
            ]
        };
}

