using System.Data;
using Dapper;
using Manam.DatabaseClient.Abstractions;
using Microsoft.Data.SqlClient;

namespace Manam.DatabaseClient.Implementations;

/// <summary>
/// SQL Dapper Broker implementation for database operations
/// </summary>
public sealed class SqlDapperBroker : ISqlDapperBroker
{
    private readonly string _connectionString;

    public SqlDapperBroker(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<int> ExecuteAsync(
        string sqlOrStoredProcedure,
        object? parameters = null,
        CommandType commandType = CommandType.StoredProcedure,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: sqlOrStoredProcedure,
            parameters: parameters,
            commandType: commandType,
            cancellationToken: cancellationToken);

        return await connection.ExecuteAsync(command);
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sqlOrStoredProcedure,
        object? parameters = null,
        CommandType commandType = CommandType.StoredProcedure,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: sqlOrStoredProcedure,
            parameters: parameters,
            commandType: commandType,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<T>(command);
        return rows.AsList().AsReadOnly();
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(
        string sqlOrStoredProcedure,
        object? parameters = null,
        CommandType commandType = CommandType.StoredProcedure,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: sqlOrStoredProcedure,
            parameters: parameters,
            commandType: commandType,
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<T>(command);
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(
        string sqlOrStoredProcedure,
        object? parameters = null,
        CommandType commandType = CommandType.StoredProcedure,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: sqlOrStoredProcedure,
            parameters: parameters,
            commandType: commandType,
            cancellationToken: cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<T>(command);
    }
}
