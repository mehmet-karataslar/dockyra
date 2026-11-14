using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dockyra.Application.Interfaces;
using Dockyra.Contracts.Models;
using Dockyra.Contracts.Results;
using Dockyra.Domain.Entities;
using Dockyra.Domain.Enums;
using Dockyra.Presentation.Wpf.DesignTokens;
using Dockyra.Presentation.Wpf.Localization;

namespace Dockyra.Presentation.Wpf.ViewModels;

public sealed record ColorPalettePreset(
    string Name,
    string AccentHex,
    string SurfaceHex,
    string Description);

public sealed record ColorSwatchItem(
    string Label,
    string ColorHex);

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IUserSettingsRepository _settingsRepository;
    private readonly ITaskbarService _taskbarService;
    private readonly ISettingsBackupService _backupService;
    private readonly IThemeService _themeService;
    private readonly ILocalizationService _localizationService;
    private readonly IGroupService? _groupService;
    private readonly IProfileService? _profileService;
    private readonly IPowerManagementService? _powerService;
    private readonly TaskbarViewModel? _taskbarViewModel;

    public SettingsViewModel(
        IUserSettingsRepository settingsRepository,
        ITaskbarService taskbarService,
        ISettingsBackupService backupService,
        IThemeService themeService,
        ILocalizationService localizationService,
        IGroupService? groupService = null,
        IProfileService? profileService = null,
        IPowerManagementService? powerService = null,
        TaskbarViewModel? taskbarViewModel = null)
    {
        _settingsRepository = settingsRepository;
        _taskbarService = taskbarService;
        _backupService = backupService;
        _themeService = themeService;
        _localizationService = localizationService;
        _groupService = groupService;
        _profileService = profileService;
        _powerService = powerService;
        _taskbarViewModel = taskbarViewModel;
    }

    public IReadOnlyList<ColorPalettePreset> ColorPresets { get; } =
    [
        new ColorPalettePreset("Mat Arduvaz & Çelik", "#5B7C99", "#F21E2229", "Göz yormayan mat koyu arduvaz ve yumuşak çelik mavisi"),
        new ColorPalettePreset("Kuzey Çamı & Adaçayı", "#5E8C76", "#F21B2421", "Doğal mat çam yeşili ve dingin adaçayı tonu"),
        new ColorPalettePreset("Gece Sisi & Lacivert", "#6482AD", "#F219212E", "Yumuşak gece laciverti ve mat okyanus mavisi"),
        new ColorPalettePreset("Sıcak Kahve & Toprak", "#A68A71", "#F2241F1C", "Doğal ahşap, espresso ve sıcak toprak tonları"),
        new ColorPalettePreset("Mat Grafit & Karbon", "#7D8590", "#F2202327", "Nötr mat karbon gri ve yumuşak gümüş vurgu"),
        new ColorPalettePreset("Pişmiş Toprak & Kil", "#B57B65", "#F2271E1C", "Sıcak kiremit, kil ve yumuşak bakır tonları"),
        new ColorPalettePreset("Zeytin Dalı & Yosun", "#7E8F69", "#F21F241D", "Doğal mat zeytin yeşili ve yumuşak yaprak tonu"),
        new ColorPalettePreset("Dingin Lavanta & Gri", "#847996", "#F2211E26", "Pastel mat lavanta ve yumuşak alacakaranlık"),
        new ColorPalettePreset("Deniz Taşı & Turkuaz", "#5A8F99", "#F21A2427", "Parlamayan mat su yeşili ve okyanus çakılı"),
        new ColorPalettePreset("Kum Taşı & Buğday", "#B89660", "#F226211A", "Sıcak mat buğday ve doğal kum taşı tonu"),
        new ColorPalettePreset("Mürdüm & Gül Kurusu", "#9E6B7B", "#F2261D21", "Mat gül kurusu ve yumuşak kadife tonu"),
        new ColorPalettePreset("İskandinav Fiyordu", "#4F7CAC", "#F2161E27", "Kuzey mat mavi ve koyu taş zemini"),
        new ColorPalettePreset("Fildişi & Kum (Açık)", "#5B7C99", "#F5F2EFE9", "Göz yormayan sıcak fildişi açık masaüstü"),
        new ColorPalettePreset("Adaçayı Beyazı (Açık)", "#5E8C76", "#F5EBF0EC", "Doğal açık adaçayı yeşili ve ferah zemin"),
        new ColorPalettePreset("Bulut Grisi (Açık)", "#64748B", "#F5EDF2F7", "Yumuşak mat bulut grisi açık görünüm"),
        new ColorPalettePreset("Sıcak Keten (Açık)", "#9C7A5B", "#F5F3EDE6", "Doğal keten beji ve sıcak kahve vurgu")
    ];

    public IReadOnlyList<ColorSwatchItem> SurfaceSwatches { get; } =
    [
        new ColorSwatchItem("Arduvaz Koyu", "#F21E2229"),
        new ColorSwatchItem("Karbon Siyah", "#F216181B"),
        new ColorSwatchItem("Gece Laciverti", "#F219212E"),
        new ColorSwatchItem("Kuzey Çamı", "#F21B2421"),
        new ColorSwatchItem("Espresso Kahve", "#F2241F1C"),
        new ColorSwatchItem("Zeytin Koyu", "#F21F241D"),
        new ColorSwatchItem("Alacakaranlık Mor", "#F2211E26"),
        new ColorSwatchItem("Deniz Derini", "#F21A2427"),
        new ColorSwatchItem("Kiremit Toprak", "#F2271E1C"),
        new ColorSwatchItem("Mürdüm Koyu", "#F2261D21"),
        new ColorSwatchItem("Fildişi Açık", "#F5F2EFE9"),
        new ColorSwatchItem("Bulut Açık", "#F5EDF2F7")
    ];

    public IReadOnlyList<ColorSwatchItem> AccentSwatches { get; } =
    [
        new ColorSwatchItem("Çelik Mavisi", "#5B7C99"),
        new ColorSwatchItem("Fiyort Mavisi", "#4F7CAC"),
        new ColorSwatchItem("Adaçayı Yeşili", "#5E8C76"),
        new ColorSwatchItem("Zeytin Yeşili", "#7E8F69"),
        new ColorSwatchItem("Deniz Taşı", "#5A8F99"),
        new ColorSwatchItem("Sıcak Toprak", "#A68A71"),
        new ColorSwatchItem("Kum Buğday", "#B89660"),
        new ColorSwatchItem("Pişmiş Kil", "#B57B65"),
        new ColorSwatchItem("Gül Kurusu", "#9E6B7B"),
        new ColorSwatchItem("Mat Lavanta", "#847996"),
        new ColorSwatchItem("Grafit Gri", "#7D8590"),
        new ColorSwatchItem("Keten Kahve", "#9C7A5B")
    ];

    public IReadOnlyList<TaskbarDockEdge> DockEdgeOptions { get; } = Enum.GetValues<TaskbarDockEdge>();
    public IReadOnlyList<TaskbarAlignment> AlignmentOptions { get; } = Enum.GetValues<TaskbarAlignment>();
    public IReadOnlyList<AutoHideMode> AutoHideOptions { get; } = Enum.GetValues<AutoHideMode>();
    public IReadOnlyList<ThemeKind> ThemeOptions { get; } = Enum.GetValues<ThemeKind>();
    public IReadOnlyList<PowerMode> PowerModeOptions { get; } = Enum.GetValues<PowerMode>();
    public IReadOnlyList<FolderViewMode> FolderViewOptions { get; } = Enum.GetValues<FolderViewMode>();

    public ObservableCollection<ApplicationGroup> EditableGroups { get; } = [];
    public ObservableCollection<ApplicationEntry> SelectedGroupApps { get; } = [];
    public ObservableCollection<PinnedTaskbarItem> AvailableShortcuts { get; } = [];
    public ObservableCollection<WorkProfile> EditableProfiles { get; } = [];

    [ObservableProperty]
    private ThemeKind _selectedTheme = ThemeKind.Dark;

    [ObservableProperty]
    private string _selectedLanguage = "tr-TR";

    [ObservableProperty]
    private string _accentColorHex = "#3B82F6";

    [ObservableProperty]
    private string _surfaceColorHex = "#EA18181B";

    [ObservableProperty]
    private TaskbarDockEdge _selectedDockEdge = TaskbarDockEdge.Bottom;

    [ObservableProperty]
    private TaskbarAlignment _selectedAlignment = TaskbarAlignment.Center;

    [ObservableProperty]
    private AutoHideMode _selectedAutoHide = AutoHideMode.OnFullscreenApp;

    [ObservableProperty]
    private int _barHeight = 52;

    [ObservableProperty]
    private int _barWidthPercent = 100;

    [ObservableProperty]
    private int _iconSize = 24;

    [ObservableProperty]
    private int _cornerRadius = 10;

    [ObservableProperty]
    private int _opacityPercent = 95;

    [ObservableProperty]
    private TaskbarIconShape _selectedIconShape = TaskbarIconShape.RoundedSquare;

    private bool _isLoading;

    [ObservableProperty]
    private bool _showStartButton = true;

    [ObservableProperty]
    private bool _showOpenWindows = true;

    [ObservableProperty]
    private bool _showClock = true;

    [ObservableProperty]
    private bool _showPowerBadge = true;

    [ObservableProperty]
    private bool _autoPowerSaveOnBattery = true;

    [ObservableProperty]
    private bool _enableAnimations = true;

    [ObservableProperty]
    private PowerMode _selectedPowerMode = PowerMode.Balanced;

    [ObservableProperty]
    private ApplicationGroup? _selectedGroup;

    [ObservableProperty]
    private string _newGroupName = string.Empty;

    [ObservableProperty]
    private string _newGroupColorHex = "#3B82F6";

    [ObservableProperty]
    private string _newAppDisplayName = string.Empty;

    [ObservableProperty]
    private string _newAppExecutablePath = string.Empty;

    [ObservableProperty]
    private WorkProfile? _selectedProfile;

    [ObservableProperty]
    private string _exportedJsonPreview = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Hoş geldiniz! Ayarlarınızı özelleştirip aşağıdan görev çubuğunu değiştirebilirsiniz.";

    [ObservableProperty]
    private string _activeTaskbarStateText = "🟢 Şu an: Orijinal Microsoft Görev Çubuğu Aktif";

    [ObservableProperty]
    private string _swapButtonText = "🔄 Görev Çubuğunu Değiştir (Dockyra'ya Geç)";

    partial void OnSelectedGroupChanged(ApplicationGroup? value)
    {
        RefreshSelectedGroupApps();
    }

    partial void OnBarHeightChanged(int value) => PushLiveGeometryUpdate();
    partial void OnBarWidthPercentChanged(int value) => PushLiveGeometryUpdate();
    partial void OnIconSizeChanged(int value) => PushLiveGeometryUpdate();
    partial void OnCornerRadiusChanged(int value) => PushLiveGeometryUpdate();
    partial void OnOpacityPercentChanged(int value) => PushLiveGeometryUpdate();
    partial void OnSelectedIconShapeChanged(TaskbarIconShape value) => PushLiveGeometryUpdate();
    partial void OnSelectedDockEdgeChanged(TaskbarDockEdge value) => PushLiveGeometryUpdate();
    partial void OnSelectedAlignmentChanged(TaskbarAlignment value) => PushLiveGeometryUpdate();
    partial void OnShowStartButtonChanged(bool value) => PushLiveGeometryUpdate();
    partial void OnShowOpenWindowsChanged(bool value) => PushLiveGeometryUpdate();
    partial void OnShowClockChanged(bool value) => PushLiveGeometryUpdate();
    partial void OnShowPowerBadgeChanged(bool value) => PushLiveGeometryUpdate();

    partial void OnSelectedLanguageChanged(string value)
    {
        if (_isLoading || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        _localizationService.ApplyLanguage(value);
        UpdateTaskbarStateBanner();
        _ = SaveAndApplyAsync();
    }

    private void PushLiveGeometryUpdate()
    {
        if (_isLoading)
        {
            return;
        }

        _taskbarViewModel?.ApplyLiveGeometryPreview(
            BarHeight,
            BarWidthPercent,
            IconSize,
            CornerRadius,
            OpacityPercent / 100.0,
            SelectedIconShape,
            SelectedDockEdge,
            SelectedAlignment,
            ShowStartButton,
            ShowOpenWindows,
            ShowClock,
            ShowPowerBadge);

        _ = SaveAndApplyAsync();
    }

    [RelayCommand]
    public void SetLanguage(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return;
        }

        SelectedLanguage = languageCode;
        _localizationService.ApplyLanguage(languageCode);
        UpdateTaskbarStateBanner();
        StatusMessage = string.Equals(languageCode, "en-US", StringComparison.OrdinalIgnoreCase)
            ? "Language switched to English."
            : "Uygulama dili Türkçe olarak değiştirildi.";
    }

    [RelayCommand]
    public void SetIconShape(string? shapeKey)
    {
        if (string.IsNullOrWhiteSpace(shapeKey))
        {
            return;
        }

        if (Enum.TryParse<TaskbarIconShape>(shapeKey, ignoreCase: true, out TaskbarIconShape parsed))
        {
            SelectedIconShape = parsed;
            StatusMessage = SelectedLanguage == "en-US"
                ? $"Icon & button shape updated to {parsed}."
                : $"İkon ve kutu şekli '{parsed}' olarak anında güncellendi.";
        }
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        _isLoading = true;
        try
        {
            UserPreferences prefs = await _settingsRepository.GetPreferencesAsync(cancellationToken);
            TaskbarLayout layout = await _taskbarService.GetCurrentLayoutAsync(cancellationToken);

            SelectedTheme = prefs.Theme;
            SelectedLanguage = prefs.LanguageCode;
            _localizationService.ApplyLanguage(SelectedLanguage);
            AccentColorHex = prefs.AccentColorHex is "#3B82F6" or "#A855F7" or "#10B981" ? "#5B7C99" : prefs.AccentColorHex;
            SurfaceColorHex = prefs.SurfaceColorHex is "#EA18181B" or "#F00F1E38" ? "#F21E2229" : prefs.SurfaceColorHex;
            AutoPowerSaveOnBattery = prefs.AutoPowerSaveOnBattery;
            EnableAnimations = prefs.EnableAnimations;
            ShowStartButton = prefs.ShowStartButton;
            ShowOpenWindows = prefs.ShowOpenWindows;
            ShowClock = prefs.ShowClock;
            ShowPowerBadge = prefs.ShowPowerBadge;
            SelectedIconShape = prefs.IconShape;
            BarWidthPercent = Math.Clamp(prefs.BarWidthPercent, 50, 100);

            SelectedDockEdge = layout.DockEdge;
            SelectedAlignment = layout.Alignment;
            SelectedAutoHide = layout.AutoHide;
            BarHeight = layout.BarHeight;
            IconSize = Math.Clamp(layout.IconSize, 18, 40);
            CornerRadius = layout.CornerRadius;
            OpacityPercent = (int)Math.Round(layout.Opacity * 100);

            if (_powerService is not null)
            {
                SelectedPowerMode = _powerService.CurrentMode;
            }
        }
        finally
        {
            _isLoading = false;
        }

        await ReloadGroupsAndShortcutsAsync(cancellationToken);
        await ReloadProfilesAsync(cancellationToken);
        UpdateTaskbarStateBanner();
    }

    private async Task ReloadGroupsAndShortcutsAsync(CancellationToken cancellationToken)
    {
        if (_groupService is null)
        {
            return;
        }

        string? previousSelectedId = SelectedGroup?.Id;
        IReadOnlyList<ApplicationGroup> groups = await _groupService.GetAllGroupsAsync(cancellationToken);
        EditableGroups.Clear();
        foreach (ApplicationGroup group in groups.OrderBy(g => g.SortOrder))
        {
            EditableGroups.Add(group);
        }

        SelectedGroup = EditableGroups.FirstOrDefault(g => g.Id == previousSelectedId)
            ?? EditableGroups.FirstOrDefault();

        AvailableShortcuts.Clear();
        if (_taskbarViewModel is not null)
        {
            foreach (PinnedTaskbarItem item in _taskbarViewModel.PinnedApps)
            {
                AvailableShortcuts.Add(item);
            }
        }
    }

    private async Task ReloadProfilesAsync(CancellationToken cancellationToken)
    {
        if (_profileService is null)
        {
            return;
        }

        IReadOnlyList<WorkProfile> profiles = await _profileService.GetProfilesAsync(cancellationToken);
        EditableProfiles.Clear();
        foreach (WorkProfile p in profiles)
        {
            EditableProfiles.Add(p);
        }

        SelectedProfile = await _profileService.GetActiveProfileAsync(cancellationToken);
    }

    private void RefreshSelectedGroupApps()
    {
        SelectedGroupApps.Clear();
        if (SelectedGroup is null)
        {
            return;
        }

        foreach (ApplicationEntry app in SelectedGroup.Applications.OrderBy(a => a.SortOrder))
        {
            SelectedGroupApps.Add(app);
        }
    }

    public void UpdateTaskbarStateBanner()
    {
        bool isEn = string.Equals(SelectedLanguage, "en-US", StringComparison.OrdinalIgnoreCase);

        if (_taskbarViewModel is null || !_taskbarViewModel.IsDockyraActive)
        {
            ActiveTaskbarStateText = isEn
                ? "🟢 Active: Original Microsoft Taskbar"
                : "🟢 Şu an: Orijinal Microsoft Görev Çubuğu Aktif";
            SwapButtonText = isEn
                ? "🔄 Switch Taskbar (Activate Dockyra)"
                : "🔄 Görev Çubuğunu Değiştir (Dockyra Görev Çubuğuna Geç)";
        }
        else if (_taskbarViewModel.IsFullReplacementMode)
        {
            ActiveTaskbarStateText = isEn
                ? "🔵 Active: Dockyra Full Taskbar"
                : "🔵 Şu an: Dockyra Tam Görev Çubuğu Aktif";
            SwapButtonText = isEn
                ? "🔄 Switch Taskbar (Return to Microsoft)"
                : "🔄 Görev Çubuğunu Değiştir (Microsoft Görev Çubuğuna Geri Dön)";
        }
        else
        {
            ActiveTaskbarStateText = isEn
                ? "🟣 Active: Dockyra + Microsoft Overlay Mode"
                : "🟣 Şu an: Dockyra + Microsoft Entegre Mod Aktif";
            SwapButtonText = isEn
                ? "🔄 Switch Taskbar (Full Dockyra Mode)"
                : "🔄 Görev Çubuğunu Değiştir (Dockyra Tam Görev Çubuğuna Geç)";
        }
    }

    [RelayCommand]
    public void SelectColorPreset(ColorPalettePreset? preset)
    {
        if (preset is null)
        {
            return;
        }

        AccentColorHex = preset.AccentHex;
        SurfaceColorHex = preset.SurfaceHex;
        if (preset.Name.Contains("Açık", StringComparison.OrdinalIgnoreCase))
        {
            SelectedTheme = ThemeKind.Light;
            _themeService.ApplyTheme(ThemeKind.Light);
        }
        else
        {
            SelectedTheme = ThemeKind.Dark;
            _themeService.ApplyTheme(ThemeKind.Dark);
        }

        _themeService.ApplyColorPalette(AccentColorHex, SurfaceColorHex);
        StatusMessage = $"'{preset.Name}' renk paleti canlı olarak uygulandı.";
    }

    [RelayCommand]
    public void ApplyCustomColors()
    {
        _themeService.ApplyTheme(SelectedTheme);
        _themeService.ApplyColorPalette(AccentColorHex, SurfaceColorHex);
        StatusMessage = "Seçilen renk tonları canlı olarak uygulandı.";
    }

    [RelayCommand]
    public void SelectSurfaceSwatch(ColorSwatchItem? swatch)
    {
        if (swatch is null)
        {
            return;
        }

        SurfaceColorHex = swatch.ColorHex;
        SelectedTheme = swatch.Label.Contains("Açık", StringComparison.OrdinalIgnoreCase)
            ? ThemeKind.Light
            : ThemeKind.Dark;
        _themeService.ApplyTheme(SelectedTheme);
        _themeService.ApplyColorPalette(AccentColorHex, SurfaceColorHex);
        StatusMessage = $"Görev çubuğu arka plan rengi '{swatch.Label}' olarak değiştirildi.";
    }

    [RelayCommand]
    public void SelectAccentSwatch(ColorSwatchItem? swatch)
    {
        if (swatch is null)
        {
            return;
        }

        AccentColorHex = swatch.ColorHex;
        _themeService.ApplyColorPalette(AccentColorHex, SurfaceColorHex);
        StatusMessage = $"Vurgu rengi '{swatch.Label}' olarak değiştirildi.";
    }

    [RelayCommand]
    public async Task SelectFolderColorSwatchAsync(ColorSwatchItem? swatch, CancellationToken cancellationToken = default)
    {
        if (swatch is null)
        {
            return;
        }

        NewGroupColorHex = swatch.ColorHex;
        if (SelectedGroup is not null && _groupService is not null)
        {
            SelectedGroup.ColorHex = swatch.ColorHex;
            await _groupService.SaveGroupAsync(SelectedGroup, cancellationToken);
            await ReloadGroupsAndShortcutsAsync(cancellationToken);
            if (_taskbarViewModel is not null)
            {
                await _taskbarViewModel.ReloadAllConfigurationAsync(cancellationToken);
            }
            StatusMessage = $"'{SelectedGroup?.Name}' klasör rengi '{swatch.Label}' olarak güncellendi.";
        }
    }

    [RelayCommand]
    public async Task SaveAndApplyAsync(CancellationToken cancellationToken = default)
    {
        UserPreferences prefs = await _settingsRepository.GetPreferencesAsync(cancellationToken);
        prefs.Theme = SelectedTheme;
        prefs.LanguageCode = SelectedLanguage;
        prefs.AccentColorHex = AccentColorHex;
        prefs.SurfaceColorHex = SurfaceColorHex;
        prefs.AutoPowerSaveOnBattery = AutoPowerSaveOnBattery;
        prefs.EnableAnimations = EnableAnimations;
        prefs.ShowStartButton = ShowStartButton;
        prefs.ShowOpenWindows = ShowOpenWindows;
        prefs.ShowClock = ShowClock;
        prefs.ShowPowerBadge = ShowPowerBadge;
        prefs.IconShape = SelectedIconShape;
        prefs.BarWidthPercent = Math.Clamp(BarWidthPercent, 50, 100);

        await _settingsRepository.SavePreferencesAsync(prefs, cancellationToken);

        TaskbarLayout layout = await _taskbarService.GetCurrentLayoutAsync(cancellationToken);
        layout.DockEdge = SelectedDockEdge;
        layout.Alignment = SelectedAlignment;
        layout.AutoHide = SelectedAutoHide;
        layout.BarHeight = Math.Clamp(BarHeight, 40, 72);
        layout.IconSize = Math.Clamp(IconSize, 24, 48);
        layout.CornerRadius = Math.Clamp(CornerRadius, 0, 24);
        layout.Opacity = Math.Clamp(OpacityPercent / 100.0, 0.50, 1.0);

        await _taskbarService.SaveLayoutAsync(layout, cancellationToken);

        _themeService.ApplyTheme(SelectedTheme);
        _themeService.ApplyColorPalette(AccentColorHex, SurfaceColorHex);
        _localizationService.ApplyLanguage(SelectedLanguage);

        if (_powerService is not null)
        {
            _powerService.SetUserOverrideMode(SelectedPowerMode);
        }

        if (_profileService is not null && SelectedProfile is not null)
        {
            await _profileService.SwitchProfileAsync(SelectedProfile.Id, cancellationToken);
        }

        if (_taskbarViewModel is not null)
        {
            await _taskbarViewModel.ReloadAllConfigurationAsync(cancellationToken);
        }

        StatusMessage = "Tüm kişiselleştirme ayarları kaydedildi ve uygulandı.";
    }

    [RelayCommand]
    public async Task SwapTaskbarSystemAsync(CancellationToken cancellationToken = default)
    {
        await SaveAndApplyAsync(cancellationToken);
        if (_taskbarViewModel is not null)
        {
            await _taskbarViewModel.SwapWithMicrosoftTaskbarAsync(cancellationToken);
            UpdateTaskbarStateBanner();
            StatusMessage = _taskbarViewModel.StatusBanner;
        }
    }

    [RelayCommand]
    public async Task ActivateDockyraFullTaskbarAsync(CancellationToken cancellationToken = default)
    {
        await SaveAndApplyAsync(cancellationToken);
        if (_taskbarViewModel is not null)
        {
            await _taskbarViewModel.ActivateDockyraTaskbarAsync(TaskbarIntegrationMode.FullReplacement, cancellationToken);
            UpdateTaskbarStateBanner();
            StatusMessage = "Microsoft görev çubuğu gizlendi; özelleştirilmiş Dockyra Tam Görev Çubuğu devreye alındı!";
        }
    }

    [RelayCommand]
    public async Task ActivateDockyraOverlayTaskbarAsync(CancellationToken cancellationToken = default)
    {
        await SaveAndApplyAsync(cancellationToken);
        if (_taskbarViewModel is not null)
        {
            await _taskbarViewModel.ActivateDockyraTaskbarAsync(TaskbarIntegrationMode.NativeOverlay, cancellationToken);
            UpdateTaskbarStateBanner();
            StatusMessage = "Dockyra, mevcut Microsoft görev çubuğunun üzerine entegre edildi!";
        }
    }

    [RelayCommand]
    public void RestoreMicrosoftTaskbar()
    {
        if (_taskbarViewModel is not null)
        {
            _taskbarViewModel.RestoreMicrosoftTaskbarOnly();
            UpdateTaskbarStateBanner();
            StatusMessage = "Orijinal Microsoft Windows 11 görev çubuğu geri yüklendi.";
        }
    }

    [RelayCommand]
    public async Task CreateNewGroupAsync(CancellationToken cancellationToken = default)
    {
        if (_groupService is null || string.IsNullOrWhiteSpace(NewGroupName))
        {
            StatusMessage = "Lütfen yeni grup için bir isim girin.";
            return;
        }

        OperationResult<ApplicationGroup> result = await _groupService.CreateGroupAsync(
            NewGroupName.Trim(),
            string.IsNullOrWhiteSpace(NewGroupColorHex) ? "#3B82F6" : NewGroupColorHex.Trim(),
            "FolderGlyph",
            cancellationToken);

        if (result.IsSuccess && result.Value is not null)
        {
            if (_profileService is not null && SelectedProfile is not null && !SelectedProfile.EnabledGroupIds.Contains(result.Value.Id))
            {
                SelectedProfile.EnabledGroupIds.Add(result.Value.Id);
            }

            NewGroupName = string.Empty;
            await ReloadGroupsAndShortcutsAsync(cancellationToken);
            SelectedGroup = EditableGroups.FirstOrDefault(g => g.Id == result.Value.Id);
            if (_taskbarViewModel is not null)
            {
                await _taskbarViewModel.ReloadAllConfigurationAsync(cancellationToken);
            }
            StatusMessage = $"'{result.Value.Name}' klasörü oluşturuldu ve görev çubuğuna eklendi.";
        }
    }

    [RelayCommand]
    public async Task DeleteSelectedGroupAsync(CancellationToken cancellationToken = default)
    {
        if (_groupService is null || SelectedGroup is null)
        {
            return;
        }

        string deletedName = SelectedGroup.Name;
        await _groupService.DeleteGroupAsync(SelectedGroup.Id, cancellationToken);
        await ReloadGroupsAndShortcutsAsync(cancellationToken);
        if (_taskbarViewModel is not null)
        {
            await _taskbarViewModel.ReloadAllConfigurationAsync(cancellationToken);
        }
        StatusMessage = $"'{deletedName}' grubu silindi.";
    }

    [RelayCommand]
    public async Task MoveSelectedGroupUpAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedGroup is null)
        {
            return;
        }

        int idx = EditableGroups.IndexOf(SelectedGroup);
        if (idx > 0)
        {
            await ReorderGroupsByDropAsync(SelectedGroup.Id, EditableGroups[idx - 1].Id, cancellationToken);
        }
    }

    [RelayCommand]
    public async Task MoveSelectedGroupDownAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedGroup is null)
        {
            return;
        }

        int idx = EditableGroups.IndexOf(SelectedGroup);
        if (idx >= 0 && idx < EditableGroups.Count - 1)
        {
            await ReorderGroupsByDropAsync(SelectedGroup.Id, EditableGroups[idx + 1].Id, cancellationToken);
        }
    }

    public async Task ReorderGroupsByDropAsync(
        string sourceGroupId,
        string targetGroupId,
        CancellationToken cancellationToken = default)
    {
        if (_groupService is null || string.Equals(sourceGroupId, targetGroupId, StringComparison.Ordinal))
        {
            return;
        }

        ApplicationGroup? source = EditableGroups.FirstOrDefault(g => g.Id == sourceGroupId);
        ApplicationGroup? target = EditableGroups.FirstOrDefault(g => g.Id == targetGroupId);
        if (source is null || target is null)
        {
            return;
        }

        int oldIndex = EditableGroups.IndexOf(source);
        int newIndex = EditableGroups.IndexOf(target);
        EditableGroups.Move(oldIndex, newIndex);

        List<string> orderedIds = EditableGroups.Select(g => g.Id).ToList();
        await _groupService.ReorderGroupsAsync(orderedIds, cancellationToken);
        SelectedGroup = source;

        if (_taskbarViewModel is not null)
        {
            await _taskbarViewModel.ReloadAllConfigurationAsync(cancellationToken);
        }

        StatusMessage = $"'{source.Name}' klasör sırası sürükle-bırak ile güncellendi.";
    }

    [RelayCommand]
    public async Task AddShortcutToSelectedGroupAsync(PinnedTaskbarItem? item, CancellationToken cancellationToken = default)
    {
        if (_groupService is null || SelectedGroup is null || item is null)
        {
            return;
        }

        if (SelectedGroup.Applications.Any(a =>
                string.Equals(a.DisplayName, item.DisplayName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(a.ExecutablePath, item.ShortcutPath, StringComparison.OrdinalIgnoreCase)))
        {
            StatusMessage = $"'{item.DisplayName}' zaten '{SelectedGroup.Name}' klasöründe bulunuyor.";
            return;
        }

        var entry = new ApplicationEntry
        {
            Id = $"app-{Guid.NewGuid():N}"[..12],
            DisplayName = item.DisplayName,
            ExecutablePath = item.ShortcutPath,
            SortOrder = SelectedGroup.Applications.Count
        };

        await _groupService.AddApplicationToGroupAsync(SelectedGroup.Id, entry, cancellationToken);
        await ReloadGroupsAndShortcutsAsync(cancellationToken);
        if (_taskbarViewModel is not null)
        {
            await _taskbarViewModel.ReloadAllConfigurationAsync(cancellationToken);
        }

        StatusMessage = $"'{item.DisplayName}' → '{SelectedGroup?.Name}' klasörüne eklendi.";
    }

    [RelayCommand]
    public async Task AddCustomExecutableToGroupAsync(CancellationToken cancellationToken = default)
    {
        if (_groupService is null || SelectedGroup is null || string.IsNullOrWhiteSpace(NewAppExecutablePath))
        {
            StatusMessage = "Lütfen geçerli bir uygulama (.exe veya .lnk) yolu seçin.";
            return;
        }

        string path = NewAppExecutablePath.Trim();
        string name = string.IsNullOrWhiteSpace(NewAppDisplayName)
            ? Path.GetFileNameWithoutExtension(path)
            : NewAppDisplayName.Trim();

        var entry = new ApplicationEntry
        {
            Id = $"app-{Guid.NewGuid():N}"[..12],
            DisplayName = name,
            ExecutablePath = path,
            SortOrder = SelectedGroup.Applications.Count
        };

        await _groupService.AddApplicationToGroupAsync(SelectedGroup.Id, entry, cancellationToken);
        NewAppDisplayName = string.Empty;
        NewAppExecutablePath = string.Empty;

        await ReloadGroupsAndShortcutsAsync(cancellationToken);
        if (_taskbarViewModel is not null)
        {
            await _taskbarViewModel.ReloadAllConfigurationAsync(cancellationToken);
        }

        StatusMessage = $"'{name}' uygulaması '{SelectedGroup?.Name}' klasörüne eklendi.";
    }

    [RelayCommand]
    public async Task RemoveAppFromSelectedGroupAsync(ApplicationEntry? app, CancellationToken cancellationToken = default)
    {
        if (_groupService is null || SelectedGroup is null || app is null)
        {
            return;
        }

        await _groupService.RemoveApplicationFromGroupAsync(SelectedGroup.Id, app.Id, cancellationToken);
        await ReloadGroupsAndShortcutsAsync(cancellationToken);
        if (_taskbarViewModel is not null)
        {
            await _taskbarViewModel.ReloadAllConfigurationAsync(cancellationToken);
        }

        StatusMessage = $"'{app.DisplayName}' klasörden çıkarıldı.";
    }

    public async Task ReorderAppsInSelectedGroupAsync(
        string sourceAppId,
        string targetAppId,
        CancellationToken cancellationToken = default)
    {
        if (_groupService is null || SelectedGroup is null || string.Equals(sourceAppId, targetAppId, StringComparison.Ordinal))
        {
            return;
        }

        ApplicationEntry? source = SelectedGroupApps.FirstOrDefault(a => a.Id == sourceAppId);
        ApplicationEntry? target = SelectedGroupApps.FirstOrDefault(a => a.Id == targetAppId);
        if (source is null || target is null)
        {
            return;
        }

        int oldIndex = SelectedGroupApps.IndexOf(source);
        int newIndex = SelectedGroupApps.IndexOf(target);
        SelectedGroupApps.Move(oldIndex, newIndex);

        SelectedGroup.Applications.Clear();
        for (int i = 0; i < SelectedGroupApps.Count; i++)
        {
            SelectedGroupApps[i].SortOrder = i;
            SelectedGroup.Applications.Add(SelectedGroupApps[i]);
        }

        await _groupService.SaveGroupAsync(SelectedGroup, cancellationToken);
        if (_taskbarViewModel is not null)
        {
            await _taskbarViewModel.ReloadAllConfigurationAsync(cancellationToken);
        }

        StatusMessage = $"'{source.DisplayName}' sıralaması sürükle-bırak ile değiştirildi.";
    }

    [RelayCommand]
    public async Task ResetLayoutAsync(CancellationToken cancellationToken = default)
    {
        await _taskbarService.ResetToDefaultLayoutAsync(cancellationToken);
        SelectedDockEdge = TaskbarDockEdge.Bottom;
        SelectedAlignment = TaskbarAlignment.Center;
        BarHeight = 52;
        IconSize = 36;
        CornerRadius = 10;
        OpacityPercent = 95;
        AccentColorHex = "#3B82F6";
        SurfaceColorHex = "#EA18181B";
        await SaveAndApplyAsync(cancellationToken);
        StatusMessage = "Görev çubuğu düzeni ve renkleri fabrika ayarlarına sıfırlandı.";
    }

    [RelayCommand]
    public async Task ExportBackupJsonAsync(CancellationToken cancellationToken = default)
    {
        OperationResult<string> result = await _backupService.ExportToJsonAsync(cancellationToken);
        if (result.IsSuccess && result.Value is not null)
        {
            ExportedJsonPreview = result.Value;
            StatusMessage = "JSON yedeği oluşturuldu.";
        }
    }

    [RelayCommand]
    public async Task ImportBackupJsonAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ExportedJsonPreview))
        {
            StatusMessage = "İçe aktarmak için geçerli bir JSON metni girin.";
            return;
        }

        OperationResult<SettingsBackupPackage> result = await _backupService.ImportFromJsonAsync(ExportedJsonPreview, cancellationToken);
        if (result.IsSuccess)
        {
            await LoadAsync(cancellationToken);
            if (_taskbarViewModel is not null)
            {
                await _taskbarViewModel.ReloadAllConfigurationAsync(cancellationToken);
            }
            StatusMessage = "JSON yedeği başarıyla geri yüklendi ve uygulandı.";
        }
        else
        {
            StatusMessage = result.Error?.Message ?? "JSON yedeği içe aktarılamadı.";
        }
    }
}
