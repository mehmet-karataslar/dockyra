using Dockyra.Contracts.Results;
using Dockyra.Domain.Entities;

namespace Dockyra.Application.Interfaces;

public interface ITaskbarLayoutRepository
{
    Task<TaskbarLayout> GetLayoutAsync(string layoutId, CancellationToken cancellationToken = default);
    Task<OperationResult> SaveLayoutAsync(TaskbarLayout layout, CancellationToken cancellationToken = default);
}

public interface IApplicationGroupRepository
{
    Task<IReadOnlyList<ApplicationGroup>> GetGroupsAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> SaveGroupAsync(ApplicationGroup group, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteGroupAsync(string groupId, CancellationToken cancellationToken = default);
}

public interface IProfileRepository
{
    Task<IReadOnlyList<WorkProfile>> GetProfilesAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> SaveProfileAsync(WorkProfile profile, CancellationToken cancellationToken = default);
}

public interface IUserSettingsRepository
{
    Task<UserPreferences> GetPreferencesAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> SavePreferencesAsync(UserPreferences preferences, CancellationToken cancellationToken = default);
}

public interface ISafeModeStateStore
{
    int RecordStartupAttempt();
    void RecordStartupSuccess();
    int GetConsecutiveFailures();
}

public sealed record SettingsBackupPackage(
    int SchemaVersion,
    DateTimeOffset ExportedAtUtc,
    UserPreferences Preferences,
    TaskbarLayout ActiveLayout,
    IReadOnlyList<ApplicationGroup> Groups,
    IReadOnlyList<WorkProfile> Profiles);

public interface ISettingsBackupService
{
    Task<OperationResult<string>> ExportToJsonAsync(CancellationToken cancellationToken = default);
    Task<OperationResult<SettingsBackupPackage>> ImportFromJsonAsync(string jsonContent, CancellationToken cancellationToken = default);
}

