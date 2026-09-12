namespace Jarvis.Infrastructure;

public sealed class SqliteOptions
{
    public const string SectionName = "Sqlite";

    public string ConnectionString { get; init; } = "Data Source=jarvis.db";
}
