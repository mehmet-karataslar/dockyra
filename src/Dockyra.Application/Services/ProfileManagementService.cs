using Dockyra.Application.Interfaces;
using Dockyra.Contracts.Results;
using Dockyra.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Dockyra.Application.Services;

public sealed class ProfileManagementService : IProfileService
{
    private readonly IProfileRepository _profileRepository;
    private readonly IUserSettingsRepository _settingsRepository;
    private readonly ILogger<ProfileManagementService> _logger;

    public ProfileManagementService(
        IProfileRepository profileRepository,
        IUserSettingsRepository settingsRepository,
        ILogger<ProfileManagementService> logger)
    {
        _profileRepository = profileRepository;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WorkProfile>> GetProfilesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<WorkProfile> profiles = await _profileRepository.GetProfilesAsync(cancellationToken).ConfigureAwait(false);
        if (profiles.Count > 0)
        {
            return profiles;
        }

        IReadOnlyList<WorkProfile> builtIn = WorkProfile.CreateBuiltInProfiles();
        foreach (WorkProfile profile in builtIn)
        {
            await _profileRepository.SaveProfileAsync(profile, cancellationToken).ConfigureAwait(false);
        }

        return builtIn;
    }

    public async Task<WorkProfile> GetActiveProfileAsync(CancellationToken cancellationToken = default)
    {
        UserPreferences prefs = await _settingsRepository.GetPreferencesAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<WorkProfile> profiles = await GetProfilesAsync(cancellationToken).ConfigureAwait(false);
        return profiles.FirstOrDefault(p => string.Equals(p.Id, prefs.ActiveProfileId, StringComparison.Ordinal))
            ?? profiles[0];
    }

    public async Task<OperationResult<WorkProfile>> SwitchProfileAsync(
        string profileId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<WorkProfile> profiles = await GetProfilesAsync(cancellationToken).ConfigureAwait(false);
        WorkProfile? target = profiles.FirstOrDefault(p => string.Equals(p.Id, profileId, StringComparison.Ordinal));
        if (target is null)
        {
            return OperationResult<WorkProfile>.Failure(
                "PROFILE_NOT_FOUND",
                $"Profile '{profileId}' was not found.",
                ErrorCategory.Validation);
        }

        UserPreferences prefs = await _settingsRepository.GetPreferencesAsync(cancellationToken).ConfigureAwait(false);
        prefs.ActiveProfileId = target.Id;
        OperationResult saveResult = await _settingsRepository.SavePreferencesAsync(prefs, cancellationToken).ConfigureAwait(false);
        if (!saveResult.IsSuccess)
        {
            return OperationResult<WorkProfile>.Failure(
                saveResult.Error?.Code ?? "PROFILE_SWITCH_FAILED",
                saveResult.Error?.Message ?? "Could not persist active profile.",
                ErrorCategory.Persistence);
        }

        _logger.LogInformation("Switched active work profile to {ProfileId} ({ProfileName})", target.Id, target.Name);
        return OperationResult<WorkProfile>.Success(target);
    }
}

