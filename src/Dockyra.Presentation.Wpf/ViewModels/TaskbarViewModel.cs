using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dockyra.Application.Interfaces;
using Dockyra.Contracts.Models;
using Dockyra.Contracts.Results;
using Dockyra.Contracts.Services;
using Dockyra.Domain.Entities;
using Dockyra.Domain.Enums;
using Dockyra.Presentation.Wpf.DesignTokens;

namespace Dockyra.Presentation.Wpf.ViewModels;

public sealed partial class UnifiedTaskbarAppItem : ObservableObject
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public string? LaunchPath { get; init; }
    public byte[]? IconBgra32 { get; init; }
    public bool IsPinned { get; init; }
    public bool IsRunning { get; init; }
    public bool IsActive { get; init; }
    public int WindowCount { get; init; }
    public WindowInfo? PrimaryWindow { get; init; }
    public PinnedTaskbarItem? PinnedSource { get; init; }
    public string TooltipText => IsRunning && PrimaryWindow is not null
        ? $"{DisplayName} — {PrimaryWindow.Title}"
        : DisplayName;
}

public sealed partial class TaskbarViewModel : ObservableObject, IDisposable
{
    private readonly ITaskbarService _taskbarService;
    private readonly IGroupService _groupService;
    private readonly IProfileService _profileService;
    private readonly IWindowManager _windowManager;
    private readonly IWindowSwitchUseCase _windowSwitchUseCase;
    private readonly IPowerManagementService _powerManagementService;
    private readonly ITaskbarVisibilityService? _taskbarVisibilityService;
    private readonly IApplicationLauncher? _applicationLauncher;
    private readonly IUserSettingsRepository? _settingsRepository;
    private readonly IThemeService? _themeService;
    private readonly IPowerMonitor? _powerMonitor;
    private bool _disposed;

    public TaskbarViewModel(
        ITaskbarService taskbarService,
        IGroupService groupService,
        IProfileService profileService,
        IWindowManager windowManager,
        IWindowSwitchUseCase windowSwitchUseCase,
        IPowerManagementService powerManagementService,
        FolderPopupViewModel folderPopupViewModel,
        ITaskbarVisibilityService? taskbarVisibilityService = null,
        IApplicationLauncher? applicationLauncher = null,
        IUserSettingsRepository? settingsRepository = null,
        IThemeService? themeService = null,
        IPowerMonitor? powerMonitor = null)
    {
        _taskbarService = taskbarService;
        _groupService = groupService;
        _profileService = profileService;
        _windowManager = windowManager;
        _windowSwitchUseCase = windowSwitchUseCase;
        _powerManagementService = powerManagementService;
        FolderPopup = folderPopupViewModel;
        _taskbarVisibilityService = taskbarVisibilityService;
        _applicationLauncher = applicationLauncher;
        _settingsRepository = settingsRepository;
        _themeService = themeService;
        _powerMonitor = powerMonitor;

        _windowManager.WindowsChanged += OnWindowsChanged;
        _powerManagementService.PowerModeChanged += OnPowerModeChanged;
        FolderPopup.GroupModified += OnFolderPopupGroupModified;
        CurrentPowerMode = _powerManagementService.CurrentMode;
    }

    private async void OnFolderPopupGroupModified(object? sender, EventArgs e)
    {
        await ReloadGroupsForActiveProfileAsync(CancellationToken.None);
        RebuildUnifiedTaskbarItems();
    }

    public event EventHandler? ConfigurationUpdated;
    public event EventHandler<bool>? TaskbarVisibilityRequested;

    public FolderPopupViewModel FolderPopup { get; }

    public ObservableCollection<ApplicationGroup> Groups { get; } = [];
    public ObservableCollection<PinnedTaskbarItem> PinnedApps { get; } = [];
    public ObservableCollection<WindowInfo> OpenWindows { get; } = [];
    public ObservableCollection<UnifiedTaskbarAppItem> UnifiedTaskbarItems { get; } = [];
    public ObservableCollection<WorkProfile> Profiles { get; } = [];

    [ObservableProperty]
    private WorkProfile? _activeProfile;

    [ObservableProperty]
    private PowerMode _currentPowerMode;

    [ObservableProperty]
    private TaskbarDockEdge _dockEdge = TaskbarDockEdge.Bottom;

    [ObservableProperty]
    private TaskbarAlignment _alignment = TaskbarAlignment.Center;

    [ObservableProperty]
    private int _barHeight = 52;

    [ObservableProperty]
    private int _barWidthPercent = 100;

    [ObservableProperty]
    private int _iconSize = 24;

    [ObservableProperty]
    private int _cornerRadius = 10;

    [ObservableProperty]
    private double _barOpacity = 0.95;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IconShapeCornerRadius))]
    [NotifyPropertyChangedFor(nameof(GroupPillCornerRadius))]
    private TaskbarIconShape _iconShape = TaskbarIconShape.RoundedSquare;

    public int IconShapeCornerRadius => IconShape switch
    {
        TaskbarIconShape.Circle => 22,
        TaskbarIconShape.RoundedSquare => 8,
        TaskbarIconShape.Square => 2,
        TaskbarIconShape.Capsule => 14,
        _ => 8
    };

    public int GroupPillCornerRadius => IconShape switch
    {
        TaskbarIconShape.Circle => 18,
        TaskbarIconShape.RoundedSquare => 8,
        TaskbarIconShape.Square => 2,
        TaskbarIconShape.Capsule => 16,
        _ => 8
    };

    [ObservableProperty]
    private bool _showStartButton = true;

    [ObservableProperty]
    private bool _showOpenWindows = true;

    [ObservableProperty]
    private bool _showClock = true;

    [ObservableProperty]
    private bool _showPowerBadge = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFullReplacementMode))]
    [NotifyPropertyChangedFor(nameof(IntegrationModeLabel))]
    private TaskbarIntegrationMode _integrationMode = TaskbarIntegrationMode.FullReplacement;

    [ObservableProperty]
    private bool _isDockyraActive = false;

    public bool IsFullReplacementMode => IntegrationMode == TaskbarIntegrationMode.FullReplacement;

    public string IntegrationModeLabel => IsFullReplacementMode
        ? "🔗 Entegre Moda Geç"
        : "🖥️ Tam Görev Çubuğu Yap";

    [ObservableProperty]
    private string _currentTimeText = DateTime.Now.ToString("HH:mm");

    [ObservableProperty]
    private string _currentDateText = DateTime.Now.ToString("dd.MM.yyyy");

    [ObservableProperty]
    private string _batteryStatusText = "🔋 %100";

    [ObservableProperty]
    private string _wifiStatusText = "📶 Wi-Fi";

    [ObservableProperty]
    private string _bluetoothStatusText = "ᛒ BT";

    [ObservableProperty]
    private string _statusBanner = "Hazır";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_taskbarVisibilityService is not null)
        {
            IReadOnlyList<PinnedTaskbarItem> nativePinned = _taskbarVisibilityService.GetNativePinnedTaskbarItems();
            PinnedApps.Clear();
            foreach (PinnedTaskbarItem pin in nativePinned)
            {
                PinnedApps.Add(pin);
            }

            await _groupService.SyncNativePinnedAppsAsync(nativePinned, cancellationToken);
        }

        await ReloadAllConfigurationAsync(cancellationToken);
        await RefreshWindowsAsync(cancellationToken);
        UpdateClockText();
    }

    public async Task ReloadAllConfigurationAsync(CancellationToken cancellationToken = default)
    {
        TaskbarLayout layout = await _taskbarService.GetCurrentLayoutAsync(cancellationToken);
        DockEdge = layout.DockEdge;
        Alignment = layout.Alignment;
        BarHeight = layout.BarHeight;
        IconSize = layout.IconSize;
        CornerRadius = layout.CornerRadius;
        BarOpacity = layout.Opacity;

        if (_settingsRepository is not null)
        {
            UserPreferences prefs = await _settingsRepository.GetPreferencesAsync(cancellationToken);
            IntegrationMode = prefs.IntegrationMode;
            ShowStartButton = prefs.ShowStartButton;
            ShowOpenWindows = prefs.ShowOpenWindows;
            ShowClock = prefs.ShowClock;
            ShowPowerBadge = prefs.ShowPowerBadge;
            IconShape = prefs.IconShape;
            BarWidthPercent = Math.Clamp(prefs.BarWidthPercent, 50, 100);

            string accent = prefs.AccentColorHex is "#3B82F6" or "#A855F7" or "#10B981" ? "#5B7C99" : prefs.AccentColorHex;
            string surface = prefs.SurfaceColorHex is "#EA18181B" or "#F00F1E38" ? "#F21E2229" : prefs.SurfaceColorHex;
            _themeService?.ApplyTheme(prefs.Theme);
            _themeService?.ApplyColorPalette(accent, surface);
        }

        IReadOnlyList<WorkProfile> profiles = await _profileService.GetProfilesAsync(cancellationToken);
        Profiles.Clear();
        foreach (WorkProfile profile in profiles)
        {
            Profiles.Add(profile);
        }

        ActiveProfile = await _profileService.GetActiveProfileAsync(cancellationToken);
        await ReloadGroupsForActiveProfileAsync(cancellationToken);

        ConfigurationUpdated?.Invoke(this, EventArgs.Empty);
    }

    public void ApplyLiveGeometryPreview(
        int barHeight,
        int barWidthPercent,
        int iconSize,
        int cornerRadius,
        double opacity,
        TaskbarIconShape iconShape,
        TaskbarDockEdge dockEdge,
        TaskbarAlignment alignment,
        bool showStart,
        bool showWindows,
        bool showClock,
        bool showPower)
    {
        BarHeight = Math.Clamp(barHeight, 40, 72);
        BarWidthPercent = Math.Clamp(barWidthPercent, 50, 100);
        IconSize = Math.Clamp(iconSize, 18, 40);
        CornerRadius = Math.Clamp(cornerRadius, 0, 24);
        BarOpacity = Math.Clamp(opacity, 0.50, 1.0);
        IconShape = iconShape;
        DockEdge = dockEdge;
        Alignment = alignment;
        ShowStartButton = showStart;
        ShowOpenWindows = showWindows;
        ShowClock = showClock;
        ShowPowerBadge = showPower;

        ConfigurationUpdated?.Invoke(this, EventArgs.Empty);
    }

    public async Task ActivateDockyraTaskbarAsync(
        TaskbarIntegrationMode mode,
        CancellationToken cancellationToken = default)
    {
        IntegrationMode = mode;
        IsDockyraActive = true;

        if (_settingsRepository is not null)
        {
            UserPreferences prefs = await _settingsRepository.GetPreferencesAsync(cancellationToken);
            prefs.IntegrationMode = mode;
            await _settingsRepository.SavePreferencesAsync(prefs, cancellationToken);
        }

        await ReloadAllConfigurationAsync(cancellationToken);

        if (_taskbarVisibilityService is not null)
        {
            bool keepMicrosoftTaskbarVisible = mode == TaskbarIntegrationMode.NativeOverlay;
            _taskbarVisibilityService.SetNativeTaskbarVisible(keepMicrosoftTaskbarVisible);
        }

        TaskbarVisibilityRequested?.Invoke(this, true);
        ConfigurationUpdated?.Invoke(this, EventArgs.Empty);
    }

    public void RestoreMicrosoftTaskbarOnly()
    {
        IsDockyraActive = false;
        _taskbarVisibilityService?.SetNativeTaskbarVisible(true);
        TaskbarVisibilityRequested?.Invoke(this, false);
        ConfigurationUpdated?.Invoke(this, EventArgs.Empty);
    }

    public async Task SwapWithMicrosoftTaskbarAsync(CancellationToken cancellationToken = default)
    {
        if (IsDockyraActive && IntegrationMode == TaskbarIntegrationMode.FullReplacement)
        {
            RestoreMicrosoftTaskbarOnly();
            StatusBanner = "Orijinal Microsoft Windows 11 görev çubuğuna geri dönüldü.";
        }
        else
        {
            await ActivateDockyraTaskbarAsync(TaskbarIntegrationMode.FullReplacement, cancellationToken);
            StatusBanner = "Dockyra Görev Çubuğu aktif edildi (Microsoft görev çubuğu gizlendi).";
        }
    }

    public async Task MoveGroupAsync(
        string sourceGroupId,
        string targetGroupId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceGroupId) ||
            string.IsNullOrWhiteSpace(targetGroupId) ||
            string.Equals(sourceGroupId, targetGroupId, StringComparison.Ordinal))
        {
            return;
        }

        ApplicationGroup? source = Groups.FirstOrDefault(g => g.Id == sourceGroupId);
        ApplicationGroup? target = Groups.FirstOrDefault(g => g.Id == targetGroupId);
        if (source is null || target is null)
        {
            return;
        }

        int oldIndex = Groups.IndexOf(source);
        int newIndex = Groups.IndexOf(target);
        if (oldIndex < 0 || newIndex < 0)
        {
            return;
        }

        Groups.Move(oldIndex, newIndex);
        List<string> orderedIds = Groups.Select(g => g.Id).ToList();
        await _groupService.ReorderGroupsAsync(orderedIds, cancellationToken);
        StatusBanner = $"'{source.Name}' grubu taşındı.";
    }

    public void UpdateClockText()
    {
        DateTime now = DateTime.Now;
        CurrentTimeText = now.ToString("HH:mm");
        CurrentDateText = now.ToString("dd.MM.yyyy");

        if (_powerMonitor is not null)
        {
            PowerState pState = _powerMonitor.GetCurrentState();
            string icon = pState.Source == PowerSourceKind.AcPower ? "⚡" : "🔋";
            BatteryStatusText = $"{icon} %{pState.BatteryLifePercent}";
        }

        try
        {
            bool netUp = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            WifiStatusText = netUp ? "📶 Wi-Fi" : "📵 Ağ Yok";
        }
        catch
        {
            WifiStatusText = "📶 Wi-Fi";
        }
    }

    public async Task DropApplicationOntoGroupAsync(
        ApplicationGroup targetGroup,
        string displayName,
        string? executablePath,
        CancellationToken cancellationToken = default)
    {
        if (targetGroup is null || string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(executablePath))
        {
            return;
        }

        if (!targetGroup.Applications.Any(a =>
                string.Equals(a.DisplayName, displayName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(a.ExecutablePath, executablePath, StringComparison.OrdinalIgnoreCase)))
        {
            var entry = new ApplicationEntry
            {
                Id = $"app-{Guid.NewGuid():N}"[..12],
                DisplayName = displayName.Trim(),
                ExecutablePath = executablePath.Trim(),
                SortOrder = targetGroup.Applications.Count
            };

            await _groupService.AddApplicationToGroupAsync(targetGroup.Id, entry, cancellationToken);
            await ReloadGroupsForActiveProfileAsync(cancellationToken);
        }

        ApplicationGroup? refreshed = Groups.FirstOrDefault(g => g.Id == targetGroup.Id) ?? targetGroup;
        FolderPopup.LoadGroup(refreshed);
        RebuildUnifiedTaskbarItems();
        StatusBanner = $"'{displayName}' → '{targetGroup.Name}' kutusuna eklendi!";
    }

    public async Task<ApplicationGroup?> CreateFolderFromTwoAppsAsync(
        UnifiedTaskbarAppItem draggedItem,
        UnifiedTaskbarAppItem targetItem,
        CancellationToken cancellationToken = default)
    {
        if (draggedItem is null || targetItem is null || string.Equals(draggedItem.Id, targetItem.Id, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string? path1 = draggedItem.LaunchPath ?? draggedItem.PrimaryWindow?.ExecutablePath;
        string? path2 = targetItem.LaunchPath ?? targetItem.PrimaryWindow?.ExecutablePath;
        if (string.IsNullOrWhiteSpace(path1) || string.IsNullOrWhiteSpace(path2))
        {
            return null;
        }

        string[] softColors = ["#5B7C99", "#5E8C76", "#847996", "#A68A71", "#5A8F99", "#9E6B7B"];
        string folderColor = softColors[Groups.Count % softColors.Length];
        int folderNumber = Groups.Count + 1;
        string folderName = $"Kutu {folderNumber}";

        OperationResult<ApplicationGroup> created = await _groupService.CreateGroupAsync(
            folderName,
            folderColor,
            "FolderGlyph",
            cancellationToken);

        if (!created.IsSuccess || created.Value is null)
        {
            return null;
        }

        string groupId = created.Value.Id;

        await _groupService.AddApplicationToGroupAsync(
            groupId,
            new ApplicationEntry
            {
                Id = $"app-{Guid.NewGuid():N}"[..12],
                DisplayName = targetItem.DisplayName,
                ExecutablePath = path2,
                SortOrder = 0
            },
            cancellationToken);

        await _groupService.AddApplicationToGroupAsync(
            groupId,
            new ApplicationEntry
            {
                Id = $"app-{Guid.NewGuid():N}"[..12],
                DisplayName = draggedItem.DisplayName,
                ExecutablePath = path1,
                SortOrder = 1
            },
            cancellationToken);

        await ReloadGroupsForActiveProfileAsync(cancellationToken);
        RebuildUnifiedTaskbarItems();

        ApplicationGroup? finalGroup = Groups.FirstOrDefault(g => g.Id == groupId);
        if (finalGroup is not null)
        {
            FolderPopup.LoadGroup(finalGroup);
            StatusBanner = $"📦 '{targetItem.DisplayName}' ve '{draggedItem.DisplayName}' birleştirilerek '{finalGroup.Name}' oluşturuldu!";
        }

        return finalGroup;
    }

    [RelayCommand]
    public async Task ClickUnifiedTaskbarItemAsync(UnifiedTaskbarAppItem? item, CancellationToken cancellationToken = default)
    {
        if (item is null)
        {
            return;
        }

        // If the app is currently running, behave like Windows 11 Taskbar: focus/restore/minimize its window!
        if (item.IsRunning && item.PrimaryWindow is not null)
        {
            await ActivateOrToggleWindowAsync(item.PrimaryWindow, cancellationToken);
            return;
        }

        // Otherwise launch the pinned shortcut or executable
        if (!string.IsNullOrWhiteSpace(item.LaunchPath) && _applicationLauncher is not null)
        {
            LaunchResult result = await _applicationLauncher.LaunchAsync(
                new ApplicationLaunchRequest(item.LaunchPath),
                cancellationToken);

            StatusBanner = result.IsSuccess
                ? $"{item.DisplayName} başlatıldı."
                : result.ErrorMessage ?? "Uygulama başlatılamadı.";
        }
    }

    [RelayCommand]
    public async Task LaunchNewInstanceOfUnifiedItemAsync(UnifiedTaskbarAppItem? item, CancellationToken cancellationToken = default)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.LaunchPath) || _applicationLauncher is null)
        {
            return;
        }

        LaunchResult result = await _applicationLauncher.LaunchAsync(
            new ApplicationLaunchRequest(item.LaunchPath),
            cancellationToken);

        StatusBanner = result.IsSuccess
            ? $"{item.DisplayName} (yeni pencere) başlatıldı."
            : result.ErrorMessage ?? "Uygulama başlatılamadı.";
    }

    [RelayCommand]
    public async Task MinimizeUnifiedItemWindowAsync(UnifiedTaskbarAppItem? item, CancellationToken cancellationToken = default)
    {
        if (item?.PrimaryWindow is null)
        {
            return;
        }

        await _windowManager.MinimizeWindowAsync(item.PrimaryWindow.Id, cancellationToken);
        await RefreshWindowsAsync(cancellationToken);
    }

    [RelayCommand]
    public async Task CloseUnifiedItemWindowAsync(UnifiedTaskbarAppItem? item, CancellationToken cancellationToken = default)
    {
        if (item?.PrimaryWindow is null)
        {
            return;
        }

        OperationResult res = await _windowManager.CloseWindowAsync(item.PrimaryWindow.Id, cancellationToken);
        StatusBanner = res.IsSuccess
            ? $"'{item.DisplayName}' penceresi kapatıldı."
            : res.Error?.Message ?? "Pencere kapatılamadı.";
        await Task.Delay(120, cancellationToken);
        await RefreshWindowsAsync(cancellationToken);
    }

    [RelayCommand]
    public async Task EndUnifiedItemTaskAsync(UnifiedTaskbarAppItem? item, CancellationToken cancellationToken = default)
    {
        if (item?.PrimaryWindow is null)
        {
            return;
        }

        OperationResult res = await _windowManager.TerminateProcessAsync(item.PrimaryWindow.ProcessId, cancellationToken);
        StatusBanner = res.IsSuccess
            ? $"⚡ '{item.DisplayName}' görevi sonlandırıldı."
            : res.Error?.Message ?? "Görev sonlandırılamadı.";
        await Task.Delay(120, cancellationToken);
        await RefreshWindowsAsync(cancellationToken);
    }

    [RelayCommand]
    public void OpenWifiSettings() => _taskbarVisibilityService?.OpenSystemQuickAction("wifi");

    [RelayCommand]
    public void OpenBluetoothSettings() => _taskbarVisibilityService?.OpenSystemQuickAction("bluetooth");

    [RelayCommand]
    public void OpenBatterySettings() => _taskbarVisibilityService?.OpenSystemQuickAction("battery");

    [RelayCommand]
    public void OpenSoundSettings() => _taskbarVisibilityService?.OpenSystemQuickAction("sound");

    [RelayCommand]
    public async Task ToggleIntegrationModeAsync(CancellationToken cancellationToken = default)
    {
        TaskbarIntegrationMode nextMode = IntegrationMode == TaskbarIntegrationMode.NativeOverlay
            ? TaskbarIntegrationMode.FullReplacement
            : TaskbarIntegrationMode.NativeOverlay;

        await ActivateDockyraTaskbarAsync(nextMode, cancellationToken);
    }

    [RelayCommand]
    public void SwitchBackToMicrosoftTaskbar()
    {
        RestoreMicrosoftTaskbarOnly();
    }

    [RelayCommand]
    public void OpenStartMenu()
    {
        _taskbarVisibilityService?.OpenNativeStartMenu();
    }

    [RelayCommand]
    public async Task LaunchPinnedAppAsync(PinnedTaskbarItem? item, CancellationToken cancellationToken = default)
    {
        if (item is null || _applicationLauncher is null)
        {
            return;
        }

        LaunchResult result = await _applicationLauncher.LaunchAsync(
            new ApplicationLaunchRequest(item.ShortcutPath),
            cancellationToken);

        StatusBanner = result.IsSuccess
            ? $"{item.DisplayName} başlatıldı."
            : result.ErrorMessage ?? "Uygulama başlatılamadı.";
    }

    [RelayCommand]
    public async Task RefreshWindowsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<WindowInfo> windows = await _windowManager.GetWindowsAsync(cancellationToken);
        ApplyWindowList(windows);
    }

    [RelayCommand]
    public void OpenGroupFolder(ApplicationGroup? group)
    {
        if (group is null)
        {
            return;
        }

        if (FolderPopup.IsOpen && string.Equals(FolderPopup.GroupTitle, group.Name, StringComparison.Ordinal))
        {
            FolderPopup.IsOpen = false;
            return;
        }

        FolderPopup.LoadGroup(group);
    }

    [RelayCommand]
    public async Task ActivateOrToggleWindowAsync(WindowInfo? window, CancellationToken cancellationToken = default)
    {
        if (window is null)
        {
            return;
        }

        OperationResult result = await _windowSwitchUseCase.SwitchOrToggleWindowAsync(window, cancellationToken);
        StatusBanner = result.IsSuccess
            ? $"{window.Title}"
            : result.Error?.Message ?? "Pencere odaklanamadı.";

        await RefreshWindowsAsync(cancellationToken);
    }

    [RelayCommand]
    public async Task SwitchProfileAsync(WorkProfile? profile, CancellationToken cancellationToken = default)
    {
        if (profile is null)
        {
            return;
        }

        OperationResult<WorkProfile> result = await _profileService.SwitchProfileAsync(profile.Id, cancellationToken);
        if (result.IsSuccess && result.Value is not null)
        {
            ActiveProfile = result.Value;
            await ReloadGroupsForActiveProfileAsync(cancellationToken);
            StatusBanner = $"Profil: {ActiveProfile.Name}";
            ConfigurationUpdated?.Invoke(this, EventArgs.Empty);
        }
    }

    [RelayCommand]
    public void CyclePowerMode()
    {
        PowerMode next = CurrentPowerMode switch
        {
            PowerMode.PowerSaver => PowerMode.Balanced,
            PowerMode.Balanced => PowerMode.Performance,
            _ => PowerMode.PowerSaver
        };

        _powerManagementService.SetUserOverrideMode(next);
        CurrentPowerMode = _powerManagementService.CurrentMode;
        StatusBanner = $"Güç Modu: {CurrentPowerMode}";
    }

    private async Task ReloadGroupsForActiveProfileAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<ApplicationGroup> allGroups = await _groupService.GetAllGroupsAsync(cancellationToken);
        Groups.Clear();

        string[] legacyDefaultIds = ["grp-pinned", "grp-dev", "grp-browsers", "grp-comm", "grp-system"];

        foreach (ApplicationGroup group in allGroups.OrderBy(g => g.SortOrder))
        {
            // Do not clutter taskbar with empty folders or legacy pre-seeded category pills
            if (group.Applications.Count == 0 || legacyDefaultIds.Contains(group.Id, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            Groups.Add(group);
        }

        RebuildUnifiedTaskbarItems();
    }

    private void OnWindowsChanged(object? sender, WindowsChangedEventArgs e)
    {
        if (System.Windows.Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess())
        {
            _ = dispatcher.InvokeAsync(() => ApplyWindowList(e.Windows));
            return;
        }

        ApplyWindowList(e.Windows);
    }

    private void OnPowerModeChanged(object? sender, PowerMode mode) =>
        CurrentPowerMode = mode;

    private void ApplyWindowList(IReadOnlyList<WindowInfo> windows)
    {
        OpenWindows.Clear();
        foreach (WindowInfo win in windows)
        {
            OpenWindows.Add(win);
        }

        RebuildUnifiedTaskbarItems();
    }

    private void RebuildUnifiedTaskbarItems()
    {
        UnifiedTaskbarItems.Clear();

        var matchedWindowIds = new HashSet<ulong>();
        var folderAppNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 0. First, match open windows against apps inside Android Folder Boxes so apps in a Folder Box STAY inside that Folder Box even when running!
        foreach (ApplicationGroup grp in Groups)
        {
            foreach (ApplicationEntry entry in grp.Applications)
            {
                string normEntry = NormalizeToken(entry.DisplayName);
                if (!string.IsNullOrWhiteSpace(normEntry))
                {
                    folderAppNames.Add(normEntry);
                }

                List<WindowInfo> folderMatchingWins = OpenWindows
                    .Where(w => IsAppNameOrPathMatchingWindow(entry.DisplayName, entry.ExecutablePath, w))
                    .ToList();

                foreach (WindowInfo fw in folderMatchingWins)
                {
                    matchedWindowIds.Add(fw.Id.Value);
                }

                WindowInfo? activeOrFirstFolderWin = folderMatchingWins.FirstOrDefault(w => w.IsActive) ?? folderMatchingWins.FirstOrDefault();
                entry.IsRunning = folderMatchingWins.Count > 0;
                entry.IsActive = folderMatchingWins.Any(w => w.IsActive);
                entry.RunningWindowHandle = activeOrFirstFolderWin?.Id.Value;
                entry.RunningProcessId = activeOrFirstFolderWin?.ProcessId;
                if (activeOrFirstFolderWin?.IconBgra32 is not null)
                {
                    entry.IconBgra32 ??= activeOrFirstFolderWin.IconBgra32;
                }
            }
        }

        // Refresh open folder popup if it is currently visible
        if (FolderPopup.IsOpen && FolderPopup.ActiveGroup is not null)
        {
            ApplicationGroup? updatedActiveGrp = Groups.FirstOrDefault(g => g.Id == FolderPopup.ActiveGroup.Id);
            if (updatedActiveGrp is not null)
            {
                FolderPopup.LoadGroup(updatedActiveGrp);
            }
        }

        // 1. Pinned apps (skip if the user placed this app inside an Android Folder Box!)
        foreach (PinnedTaskbarItem pin in PinnedApps)
        {
            string normPin = NormalizeToken(pin.DisplayName);
            if (folderAppNames.Contains(normPin))
            {
                continue;
            }

            List<WindowInfo> matchingWindows = OpenWindows
                .Where(w => !matchedWindowIds.Contains(w.Id.Value) && IsPinMatchingWindow(pin, w))
                .ToList();

            foreach (WindowInfo mw in matchingWindows)
            {
                matchedWindowIds.Add(mw.Id.Value);
            }

            WindowInfo? activeOrFirst = matchingWindows.FirstOrDefault(w => w.IsActive) ?? matchingWindows.FirstOrDefault();
            byte[]? bestIcon = activeOrFirst?.IconBgra32 ?? pin.IconBgra32;

            UnifiedTaskbarItems.Add(new UnifiedTaskbarAppItem
            {
                Id = pin.Id,
                DisplayName = activeOrFirst is not null ? activeOrFirst.Title : pin.DisplayName,
                LaunchPath = pin.ShortcutPath,
                IconBgra32 = bestIcon,
                IsPinned = true,
                IsRunning = matchingWindows.Count > 0,
                IsActive = matchingWindows.Any(w => w.IsActive),
                WindowCount = matchingWindows.Count,
                PrimaryWindow = activeOrFirst,
                PinnedSource = pin
            });
        }

        // 2. Remaining open windows that are neither inside a Folder Box nor matched to a Pinned icon
        foreach (WindowInfo win in OpenWindows)
        {
            if (matchedWindowIds.Contains(win.Id.Value))
            {
                continue;
            }

            UnifiedTaskbarItems.Add(new UnifiedTaskbarAppItem
            {
                Id = $"win-{win.Id.Value}",
                DisplayName = win.Title,
                LaunchPath = win.ExecutablePath,
                IconBgra32 = win.IconBgra32,
                IsPinned = false,
                IsRunning = true,
                IsActive = win.IsActive,
                WindowCount = 1,
                PrimaryWindow = win
            });
        }
    }

    private static bool IsAppNameOrPathMatchingWindow(string displayName, string? executableOrShortcutPath, WindowInfo win)
    {
        var syntheticPin = new PinnedTaskbarItem("tmp", displayName, executableOrShortcutPath ?? string.Empty, "tmp");
        if (IsPinMatchingWindow(syntheticPin, win))
        {
            return true;
        }

        string titleNorm = NormalizeToken(win.Title);
        string nameNorm = NormalizeToken(displayName);
        if (nameNorm.Length >= 4 && titleNorm.Contains(nameNorm, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(executableOrShortcutPath))
        {
            string entryExeNorm = NormalizeToken(Path.GetFileNameWithoutExtension(executableOrShortcutPath));
            string winProcNorm = NormalizeToken(win.ProcessName);
            if (!string.IsNullOrEmpty(entryExeNorm) && string.Equals(entryExeNorm, winProcNorm, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPinMatchingWindow(PinnedTaskbarItem pin, WindowInfo win)
    {
        string pinNorm = NormalizeToken(pin.DisplayName);
        string procNorm = NormalizeToken(win.ProcessName);
        string exeNorm = !string.IsNullOrWhiteSpace(win.ExecutablePath)
            ? NormalizeToken(Path.GetFileNameWithoutExtension(win.ExecutablePath))
            : string.Empty;

        if (string.IsNullOrEmpty(pinNorm))
        {
            return false;
        }

        if (string.Equals(pinNorm, procNorm, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(pinNorm, exeNorm, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (pinNorm.Length >= 4 && procNorm.Length >= 4 &&
            (pinNorm.Contains(procNorm, StringComparison.OrdinalIgnoreCase) ||
             procNorm.Contains(pinNorm, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Common Windows 11 app aliases
        if ((pinNorm.Contains("visualstudiocode") || pinNorm == "vscode") && procNorm == "code")
        {
            return true;
        }
        if ((pinNorm.Contains("fileexplorer") || pinNorm.Contains("dosyagezgini") || pinNorm.Contains("windowsexplorer")) && procNorm == "explorer")
        {
            return true;
        }
        if (pinNorm.Contains("edge") && procNorm == "msedge")
        {
            return true;
        }
        if (pinNorm.Contains("chrome") && procNorm == "chrome")
        {
            return true;
        }
        if (pinNorm.Contains("terminal") && procNorm == "windowsterminal")
        {
            return true;
        }

        return false;
    }

    private static string NormalizeToken(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        return new string(input.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _windowManager.WindowsChanged -= OnWindowsChanged;
        _powerManagementService.PowerModeChanged -= OnPowerModeChanged;
        _taskbarVisibilityService?.SetNativeTaskbarVisible(true);
    }
}
