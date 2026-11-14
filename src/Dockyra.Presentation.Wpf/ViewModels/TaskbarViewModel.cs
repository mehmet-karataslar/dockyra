using System.Collections.ObjectModel;
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
        CurrentPowerMode = _powerManagementService.CurrentMode;
    }

    public event EventHandler? ConfigurationUpdated;
    public event EventHandler<bool>? TaskbarVisibilityRequested;

    public FolderPopupViewModel FolderPopup { get; }

    public ObservableCollection<ApplicationGroup> Groups { get; } = [];
    public ObservableCollection<PinnedTaskbarItem> PinnedApps { get; } = [];
    public ObservableCollection<WindowInfo> OpenWindows { get; } = [];
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
        StatusBanner = $"'{displayName}' → '{targetGroup.Name}' kutusuna eklendi!";
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

        HashSet<string>? filterIds = ActiveProfile is { EnabledGroupIds.Count: > 0 }
            ? new HashSet<string>(ActiveProfile.EnabledGroupIds, StringComparer.Ordinal)
            : null;

        filterIds?.Add("grp-pinned");

        foreach (ApplicationGroup group in allGroups.OrderBy(g => g.SortOrder))
        {
            if (filterIds is null || filterIds.Contains(group.Id))
            {
                Groups.Add(group);
            }
        }
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
