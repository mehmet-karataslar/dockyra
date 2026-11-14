using System.Text.Json;
using Dockyra.Application.Interfaces;
using Dockyra.Contracts.Results;
using Dockyra.Domain.Entities;
using Dockyra.Domain.Rules;

namespace Dockyra.Infrastructure.Backup;

public sealed class JsonSettingsBackupService : ISettingsBackupService
{
    public const int CurrentBackupSchemaVersion = 1;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IUserSettingsRepository _settingsRepository;
    private readonly ITaskbarLayoutRepository _layoutRepository;
    private readonly IApplicationGroupRepository _groupRepository;
    private readonly IProfileRepository _profileRepository;

    public JsonSettingsBackupService(
        IUserSettingsRepository settingsRepository,
        ITaskbarLayoutRepository layoutRepository,
        IApplicationGroupRepository groupRepository,
        IProfileRepository profileRepository)
    {
        _settingsRepository = settingsRepository;
        _layoutRepository = layoutRepository;
        _groupRepository = groupRepository;
        _profileRepository = profileRepository;
    }

    public async Task<OperationResult<string>> ExportToJsonAsync(CancellationToken cancellationToken = default)
    {
        UserPreferences prefs = await _settingsRepository.GetPreferencesAsync(cancellationToken).ConfigureAwait(false);
        TaskbarLayout layout = await _layoutRepository.GetLayoutAsync("default-layout", cancellationToken).ConfigureAwait(false);
        IReadOnlyList<ApplicationGroup> groups = await _groupRepository.GetGroupsAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<WorkProfile> profiles = await _profileRepository.GetProfilesAsync(cancellationToken).ConfigureAwait(false);

        var package = new SettingsBackupPackage(
            CurrentBackupSchemaVersion,
            DateTimeOffset.UtcNow,
            prefs,
            layout,
            groups,
            profiles);

        string json = JsonSerializer.Serialize(package, JsonOptions);
        return OperationResult<string>.Success(json);
    }

    public async Task<OperationResult<SettingsBackupPackage>> ImportFromJsonAsync(
        string jsonContent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return OperationResult<SettingsBackupPackage>.Failure(
                "EMPTY_BACKUP_JSON",
                "Backup JSON content cannot be empty.",
                ErrorCategory.Validation);
        }

        SettingsBackupPackage? package;
        try
        {
            package = JsonSerializer.Deserialize<SettingsBackupPackage>(jsonContent, JsonOptions);
        }
        catch (JsonException ex)
        {
            return OperationResult<SettingsBackupPackage>.Failure(
                "INVALID_BACKUP_JSON",
                "Backup JSON could not be parsed.",
                ErrorCategory.Validation,
                ex.Message);
        }

        if (package is null || package.SchemaVersion != CurrentBackupSchemaVersion)
        {
            return OperationResult<SettingsBackupPackage>.Failure(
                "UNSUPPORTED_BACKUP_SCHEMA",
                $"Unsupported backup schema version. Expected version {CurrentBackupSchemaVersion}.",
                ErrorCategory.Validation);
        }

        if (!TaskbarLayoutValidator.Validate(package.ActiveLayout, out IReadOnlyList<string> layoutErrors))
        {
            return OperationResult<SettingsBackupPackage>.Failure(
                "INVALID_BACKUP_LAYOUT",
                string.Join("; ", layoutErrors),
                ErrorCategory.Validation);
        }

        await _settingsRepository.SavePreferencesAsync(package.Preferences, cancellationToken).ConfigureAwait(false);
        await _layoutRepository.SaveLayoutAsync(package.ActiveLayout, cancellationToken).ConfigureAwait(false);

        foreach (ApplicationGroup group in package.Groups)
        {
            await _groupRepository.SaveGroupAsync(group, cancellationToken).ConfigureAwait(false);
        }

        foreach (WorkProfile profile in package.Profiles)
        {
            await _profileRepository.SaveProfileAsync(profile, cancellationToken).ConfigureAwait(false);
        }

        return OperationResult<SettingsBackupPackage>.Success(package);
    }
}

