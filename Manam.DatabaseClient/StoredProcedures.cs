using System.Data;

namespace Manam.DatabaseClient;

/// <summary>
/// SQL stored procedure names constants
/// </summary>
public static class StoredProcedures
{
    // User procedures
    public const string SpGetUserById = "sp_GetUserById";
    public const string SpGetUserByUsername = "sp_GetUserByUsername";
    public const string SpGetUserByEmail = "sp_GetUserByEmail";
    public const string SpCreateUser = "sp_CreateUser";
    public const string SpUpdateUser = "sp_UpdateUser";
    public const string SpDeleteUser = "sp_DeleteUser";
    public const string SpGetAllUsers = "sp_GetAllUsers";
    public const string SpUpdateUserLockout = "sp_UpdateUserLockout";
    public const string SpResetFailedLoginAttempts = "sp_ResetFailedLoginAttempts";
}

/// <summary>
/// Database parameters builder
/// </summary>
public class DatabaseParameters
{
    private readonly Dictionary<string, object?> _parameters = new();

    public DatabaseParameters Add(string name, object? value)
    {
        _parameters[name] = value;
        return this;
    }

    public object Build() => _parameters;

    /// <summary>
    /// Converts DatabaseParameters to a dictionary for use with Dapper
    /// </summary>
    public static explicit operator Dictionary<string, object?>(DatabaseParameters parameters) => parameters._parameters;
}
