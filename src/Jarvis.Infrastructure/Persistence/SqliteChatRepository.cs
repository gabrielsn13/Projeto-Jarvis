using Jarvis.Application.Abstractions;
using Jarvis.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace Jarvis.Infrastructure.Persistence;

public sealed class SqliteChatRepository(IOptions<JarvisOptions> options) : IChatRepository
{
    private readonly string _connectionString = $"Data Source={options.Value.DatabasePath}";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS chat_messages (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                role TEXT NOT NULL,
                content TEXT NOT NULL,
                created_at TEXT NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO chat_messages (role, content, created_at)
            VALUES ($role, $content, $createdAt);
            """;

        command.Parameters.AddWithValue("$role", message.Role);
        command.Parameters.AddWithValue("$content", message.Content);
        command.Parameters.AddWithValue("$createdAt", message.CreatedAt.UtcDateTime.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ChatMessage>> GetRecentMessagesAsync(int limit, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT role, content, created_at
            FROM chat_messages
            ORDER BY id DESC
            LIMIT $limit;
            """;
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
