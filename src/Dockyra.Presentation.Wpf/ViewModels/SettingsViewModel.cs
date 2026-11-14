using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dockyra.Application.Interfaces;
using Dockyra.Contracts.Results;
using Dockyra.Domain.Entities;
using Dockyra.Domain.Enums;
using Dockyra.Presentation.Wpf.DesignTokens;
using Dockyra.Presentation.Wpf.Localization;

namespace Dockyra.Presentation.Wpf.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IUserSettingsRepository _settingsRepository;
    private readonly ITaskbarService _taskbarService;
    private readonly ISettingsBackupService _backupService;
    private readonly IThemeService _themeService;
    private readonly ILocalizationService _localizationService;

    public SettingsViewModel(
        IUserSettingsRepository settingsRepository,
        ITaskbarService taskbarService,
        ISettingsBackupService backupService,
        IThemeService themeService,
        ILocalizationService localizationService)
    {
        _settingsRepository = settingsRepository;
        _taskbarService = taskbarService;
        _backupService = backupService;
        _themeService = themeService;
        _localizationService = localizationService;
    }

    [ObservableProperty]
    private ThemeKind _selectedTheme = ThemeKind.Dark;

    [ObservableProperty]
    private string _selectedLanguage = "tr-TR";

    [ObservableProperty]
    private TaskbarDockEdge _selectedDockEdge = TaskbarDockEdge.Bottom;

    [ObservableProperty]
    private bool _autoPowerSaveOnBattery = true;

    [ObservableProperty]
    private string _exportedJsonPreview = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        UserPreferences prefs = await _settingsRepository.GetPreferencesAsync(cancellationToken);
        TaskbarLayout layout = await _taskbarService.GetCurrentLayoutAsync(cancellationToken);

        SelectedTheme = prefs.Theme;
        SelectedLanguage = prefs.LanguageCode;
        AutoPowerSaveOnBattery = prefs.AutoPowerSaveOnBattery;
        SelectedDockEdge = layout.DockEdge;
    }

    [RelayCommand]
    public async Task SaveAndApplyAsync(CancellationToken cancellationToken = default)
    {
        UserPreferences prefs = await _settingsRepository.GetPreferencesAsync(cancellationToken);
        prefs.Theme = SelectedTheme;
        prefs.LanguageCode = SelectedLanguage;
        prefs.AutoPowerSaveOnBattery = AutoPowerSaveOnBattery;

        await _settingsRepository.SavePreferencesAsync(prefs, cancellationToken);
        await _taskbarService.UpdateDockEdgeAsync(SelectedDockEdge, cancellationToken);

        _themeService.ApplyTheme(SelectedTheme);
        _localizationService.ApplyLanguage(SelectedLanguage);
        StatusMessage = "Ayarlar kaydedildi ve uygulandı.";
    }

    [RelayCommand]
    public async Task ResetLayoutAsync(CancellationToken cancellationToken = default)
    {
        await _taskbarService.ResetToDefaultLayoutAsync(cancellationToken);
        SelectedDockEdge = TaskbarDockEdge.Bottom;
        StatusMessage = "Görev çubuğu düzeni varsayılana sıfırlandı.";
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
}

