using System.Text.Json;
using Dockyra.Application.Interfaces;
using Dockyra.Contracts.Results;
using Dockyra.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace Dockyra.Infrastructure.Persistence;

public sealed class SqliteTaskbarLayoutRepository : ITaskbarLayoutRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteTaskbarLayoutRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<TaskbarLayout> GetLayoutAsync(string layoutId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT JsonPayload FROM TaskbarLayouts WHERE Id = $id LIMIT 1;";
        cmd.Parameters.AddWithValue("$id", layoutId);

        object? result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (result is string json && !string.IsNullOrWhiteSpace(json))
        {
            TaskbarLayout? parsed = JsonSerializer.Deserialize<TaskbarLayout>(json, JsonOptions);
            if (parsed is not null)
            {
                return parsed;
            }
        }

        TaskbarLayout defaultLayout = TaskbarLayout.CreateDefault();
        await SaveLayoutAsync(defaultLayout, cancellationToken).ConfigureAwait(false);
        return defaultLayout;
    }

    public async Task<OperationResult> SaveLayoutAsync(TaskbarLayout layout, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(layout);
        string payload = JsonSerializer.Serialize(layout, JsonOptions);

        await using SqliteConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO TaskbarLayouts (Id, Name, JsonPayload, UpdatedAtUtc)
            VALUES ($id, $name, $payload, $updatedAt)
            ON CONFLICT(Id) DO UPDATE SET
                Name = excluded.Name,
                JsonPayload = excluded.JsonPayload,
                UpdatedAtUtc = excluded.UpdatedAtUtc;
            """;
        cmd.Parameters.AddWithValue("$id", layout.Id);
        cmd.Parameters.AddWithValue("$name", layout.Name);
        cmd.Parameters.AddWithValue("$payload", payload);
        cmd.Parameters.AddWithValue("$updatedAt", DateTimeOffset.UtcNow.ToString("O"));

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return OperationResult.Success();
    }
}

public sealed class SqliteApplicationGroupRepository : IApplicationGroupRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteApplicationGroupRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ApplicationGroup>> GetGroupsAsync(CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT JsonPayload FROM ApplicationGroups ORDER BY SortOrder ASC;";

        List<ApplicationGroup> groups = [];
        await using SqliteDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            string json = reader.GetString(0);
            ApplicationGroup? group = JsonSerializer.Deserialize<ApplicationGroup>(json, JsonOptions);
            if (group is not null)
            {
                groups.Add(group);
            }
        }

        return groups;
    }

    public async Task<OperationResult> SaveGroupAsync(ApplicationGroup group, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);
        string payload = JsonSerializer.Serialize(group, JsonOptions);

        await using SqliteConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO ApplicationGroups (Id, Name, SortOrder, JsonPayload, UpdatedAtUtc)
            VALUES ($id, $name, $sortOrder, $payload, $updatedAt)
            ON CONFLICT(Id) DO UPDATE SET
                Name = excluded.Name,
                SortOrder = excluded.SortOrder,
                JsonPayload = excluded.JsonPayload,
                UpdatedAtUtc = excluded.UpdatedAtUtc;
            """;
        cmd.Parameters.AddWithValue("$id", group.Id);
        cmd.Parameters.AddWithValue("$name", group.Name);
        cmd.Parameters.AddWithValue("$sortOrder", group.SortOrder);
        cmd.Parameters.AddWithValue("$payload", payload);
        cmd.Parameters.AddWithValue("$updatedAt", DateTimeOffset.UtcNow.ToString("O"));

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return OperationResult.Success();
    }

    public async Task<OperationResult> DeleteGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM ApplicationGroups WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", groupId);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return OperationResult.Success();
    }
}

public sealed class SqliteProfileRepository : IProfileRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteProfileRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<WorkProfile>> GetProfilesAsync(CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT JsonPayload FROM WorkProfiles ORDER BY SortOrder ASC;";

        List<WorkProfile> profiles = [];
        await using SqliteDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            string json = reader.GetString(0);
            WorkProfile? profile = JsonSerializer.Deserialize<WorkProfile>(json, JsonOptions);
            if (profile is not null)
            {
                profiles.Add(profile);
            }
        }

        return profiles;
    }

    public async Task<OperationResult> SaveProfileAsync(WorkProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        string payload = JsonSerializer.Serialize(profile, JsonOptions);

        await using SqliteConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO WorkProfiles (Id, Name, SortOrder, JsonPayload, UpdatedAtUtc)
            VALUES ($id, $name, $sortOrder, $payload, $updatedAt)
            ON CONFLICT(Id) DO UPDATE SET
                Name = excluded.Name,
                SortOrder = excluded.SortOrder,
                JsonPayload = excluded.JsonPayload,
                UpdatedAtUtc = excluded.UpdatedAtUtc;
            """;
        cmd.Parameters.AddWithValue("$id", profile.Id);
        cmd.Parameters.AddWithValue("$name", profile.Name);
        cmd.Parameters.AddWithValue("$sortOrder", profile.SortOrder);
        cmd.Parameters.AddWithValue("$payload", payload);
        cmd.Parameters.AddWithValue("$updatedAt", DateTimeOffset.UtcNow.ToString("O"));

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return OperationResult.Success();
    }
}

public sealed class SqliteUserSettingsRepository : IUserSettingsRepository
{
    private const string PrimaryKey = "user-preferences";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteUserSettingsRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserPreferences> GetPreferencesAsync(CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT JsonPayload FROM UserSettings WHERE SettingKey = $key LIMIT 1;";
        cmd.Parameters.AddWithValue("$key", PrimaryKey);

        object? scalar = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (scalar is string json && !string.IsNullOrWhiteSpace(json))
        {
            UserPreferences? prefs = JsonSerializer.Deserialize<UserPreferences>(json, JsonOptions);
            if (prefs is not null)
            {
                return prefs;
            }
        }

        var defaults = new UserPreferences();
        await SavePreferencesAsync(defaults, cancellationToken).ConfigureAwait(false);
        return defaults;
    }

    public async Task<OperationResult> SavePreferencesAsync(UserPreferences preferences, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        string payload = JsonSerializer.Serialize(preferences, JsonOptions);

        await using SqliteConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO UserSettings (SettingKey, JsonPayload, UpdatedAtUtc)
            VALUES ($key, $payload, $updatedAt)
            ON CONFLICT(SettingKey) DO UPDATE SET
                JsonPayload = excluded.JsonPayload,
                UpdatedAtUtc = excluded.UpdatedAtUtc;
            """;
        cmd.Parameters.AddWithValue("$key", PrimaryKey);
        cmd.Parameters.AddWithValue("$payload", payload);
        cmd.Parameters.AddWithValue("$updatedAt", DateTimeOffset.UtcNow.ToString("O"));

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return OperationResult.Success();
    }
}

