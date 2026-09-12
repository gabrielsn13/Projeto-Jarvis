using Jarvis.Application.Abstractions;
using Jarvis.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace Jarvis.Infrastructure.Persistence;

public sealed class SqliteChatHistoryRepository(IOptions<SqliteOptions> options) : IChatHistoryRepository
{
    private readonly string _connectionString = options.Value.ConnectionString;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS messages (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                session_id TEXT NOT NULL,
                role TEXT NOT NULL,
                content TEXT NOT NULL,
                created_at TEXT NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task AddMessageAsync(string sessionId, ChatMessage message, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO messages (session_id, role, content, created_at)
            VALUES ($sessionId, $role, $content, $createdAt);
            """;

        command.Parameters.AddWithValue("$sessionId", sessionId);
        command.Parameters.AddWithValue("$role", message.Role);
        command.Parameters.AddWithValue("$content", message.Content);
        command.Parameters.AddWithValue("$createdAt", message.CreatedAt.UtcDateTime.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ChatMessage>> GetRecentMessagesAsync(string sessionId, int limit, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT role, content, created_at
            FROM messages
            WHERE session_id = $sessionId
            ORDER BY id DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$sessionId", sessionId);
        command.Parameters.AddWithValue("$limit", limit);

        var result = new List<ChatMessage>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new ChatMessage
            {
                Role = reader.GetString(0),
                Content = reader.GetString(1),
                CreatedAt = DateTimeOffset.Parse(reader.GetString(2))
            });
        }

        result.Reverse();
        return result;
    }
}
