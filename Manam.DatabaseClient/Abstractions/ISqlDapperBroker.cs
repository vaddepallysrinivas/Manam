using System.Data;

namespace Manam.DatabaseClient.Abstractions;

/// <summary>
/// Interface for SQL/Dapper database operations
/// </summary>
public interface ISqlDapperBroker
{
    Task<int> ExecuteAsync(
        string sqlOrStoredProcedure,
        object? parameters = null,
        CommandType commandType = CommandType.StoredProcedure,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> QueryAsync<T>(
        string sqlOrStoredProcedure,
        object? parameters = null,
        CommandType commandType = CommandType.StoredProcedure,
        CancellationToken cancellationToken = default);

    Task<T?> QuerySingleOrDefaultAsync<T>(
        string sqlOrStoredProcedure,
        object? parameters = null,
        CommandType commandType = CommandType.StoredProcedure,
        CancellationToken cancellationToken = default);

    Task<T?> QueryFirstOrDefaultAsync<T>(
        string sqlOrStoredProcedure,
        object? parameters = null,
        CommandType commandType = CommandType.StoredProcedure,
        CancellationToken cancellationToken = default);
}
