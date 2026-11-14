using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Dockyra.Infrastructure.Persistence;

public sealed class SqliteConnectionFactory
{
    public string ConnectionString { get; }

    public SqliteConnectionFactory(string? databaseFilePath = null)
    {
        if (string.IsNullOrWhiteSpace(databaseFilePath))
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dir = Path.Combine(appData, "Dockyra");
            Directory.CreateDirectory(dir);
            databaseFilePath = Path.Combine(dir, "dockyra.db");
        }
        else if (!databaseFilePath.Contains(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            string? dir = Path.GetDirectoryName(databaseFilePath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = databaseFilePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = true
        };

        ConnectionString = builder.ToString();
    }

    public SqliteConnection CreateConnection() => new(ConnectionString);
}

public sealed class DatabaseMigrator
{
    public const int CurrentSchemaVersion = 1;
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseMigrator> _logger;

    public DatabaseMigrator(
        SqliteConnectionFactory connectionFactory,
        ILogger<DatabaseMigrator> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using SqliteCommand pragmaCmd = connection.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
        await pragmaCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        await using SqliteCommand migrationTableCmd = connection.CreateCommand();
        migrationTableCmd.CommandText = """
            CREATE TABLE IF NOT EXISTS SchemaMigrations (
                Version INTEGER PRIMARY KEY,
                AppliedAtUtc TEXT NOT NULL
            );
            """;
        await migrationTableCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        int currentVersion = await GetCurrentVersionAsync(connection, cancellationToken).ConfigureAwait(false);
        if (currentVersion < 1)
        {
            await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await using SqliteCommand v1Cmd = connection.CreateCommand();
            v1Cmd.Transaction = transaction;
            v1Cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS UserSettings (
                    SettingKey TEXT PRIMARY KEY,
                    JsonPayload TEXT NOT NULL,
                    UpdatedAtUtc TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS TaskbarLayouts (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    JsonPayload TEXT NOT NULL,
                    UpdatedAtUtc TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS ApplicationGroups (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    SortOrder INTEGER NOT NULL,
                    JsonPayload TEXT NOT NULL,
                    UpdatedAtUtc TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS WorkProfiles (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    SortOrder INTEGER NOT NULL,
                    JsonPayload TEXT NOT NULL,
                    UpdatedAtUtc TEXT NOT NULL
                );

                INSERT INTO SchemaMigrations (Version, AppliedAtUtc)
                VALUES (1, $appliedAt);
                """;
            v1Cmd.Parameters.AddWithValue("$appliedAt", DateTimeOffset.UtcNow.ToString("O"));
            await v1Cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Applied SQLite schema migration version 1");
        }
    }

    public async Task<int> GetAppliedVersionAsync(CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return await GetCurrentVersionAsync(connection, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<int> GetCurrentVersionAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COALESCE(MAX(Version), 0) FROM SchemaMigrations;";
        object? scalar = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(scalar);
    }
}

