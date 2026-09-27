# StoredProcedures Integration Guide

## Overview

The `StoredProcedures.cs` class provides a clean, async data access layer that wraps all 15 stored procedures in the Manam database. It's synchronized with the actual database schema on `DESKTOP-O4ATQ75\MSSQLSERVER2\Manam`.

---

## 📁 Files Created

### 1. **Manam.Data\StoredProcedures.cs** (26.4 KB)
- Main data access class implementing `IStoredProcedures`
- 15 async methods for all stored procedures
- Full logging and error handling
- Parameter mapping and output parameter handling
- Connection management

### 2. **Manam.Data\IStoredProcedures.cs** (3.1 KB)
- Interface defining all data access methods
- XML documentation for each method
- Type-safe contract for dependency injection

### 3. **Manam.Data\Extensions\DataServiceExtensions.cs** (2.5 KB)
- DI extension methods for service registration
- Supports configuration-based and manual connection string setup
- Singleton registration for optimal performance

---

## 🗄️ Database Synchronization

### Tables Synced (7)
```
✓ maRoles
✓ maExternalProviders
✓ maUsers
✓ maUserRoles
✓ maUserExternalAuth
✓ maUserLoginHistory
✓ maPasswordResetTokens
```

### Stored Procedures Synced (15)
```
✓ sp_maLoginUser
✓ sp_maInsertUser
✓ sp_maUpdateUser
✓ sp_maGetUserById
✓ sp_maGetUserByEmail
✓ sp_maGetAllUsers
✓ sp_maSoftDeleteUser
✓ sp_maGetUserRoles
✓ sp_maAssignRoleToUser
✓ sp_maGetUserWithRoles
✓ sp_maAddExternalAuth
✓ sp_maLogLoginHistory
✓ sp_maCreatePasswordResetToken
✓ sp_maResetPassword
✓ sp_maVerifyEmail
```

---

## 🚀 Setup Instructions

### Step 1: Add NuGet Package (if needed)
```bash
dotnet add package Microsoft.Data.SqlClient
```

### Step 2: Register in Program.cs
```csharp
using Manam.Data.Extensions;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Option 1: Using configuration (recommended)
builder.Services.AddDataServices(builder.Configuration);

// Option 2: Using direct connection string
// builder.Services.AddDataServices("Server=DESKTOP-O4ATQ75\\MSSQLSERVER2;Database=Manam;Integrated Security=true;");

var app = builder.Build();
app.Run();
```

### Step 3: Configure Connection String in appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-O4ATQ75\\MSSQLSERVER2;Database=Manam;Integrated Security=true;"
  }
}
```

---

## 💡 Usage Examples

### Inject Into Service
```csharp
public class AuthenticationService
{
    private readonly IStoredProcedures _sprocedures;
    
    public AuthenticationService(IStoredProcedures storedProcedures)
    {
        _sprocedures = storedProcedures;
    }
}
```

### Call Stored Procedures

#### Example 1: Login User
```csharp
public async Task<LoginResponse> LoginAsync(LoginRequest request)
{
    var result = await _sprocedures.sp_maLoginUserAsync(
        email: request.Email,
        passwordHash: HashPassword(request.Password),
        ipAddress: request.IpAddress,
        userAgent: request.UserAgent
    );

    if (result.Rows.Count == 0)
        return new LoginResponse { Success = false };

    var row = result.Rows[0];
    var userId = (int)row["UserId"];
    
    return new LoginResponse 
    { 
        Success = true,
        UserId = userId,
        Token = GenerateJwtToken(userId)
    };
}
```

#### Example 2: Register New User
```csharp
public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
{
    var userId = await _sprocedures.sp_maInsertUserAsync(
        username: request.Username,
        email: request.Email,
        passwordHash: HashPassword(request.Password),
        firstName: request.FirstName,
        lastName: request.LastName,
        phoneNumber: request.PhoneNumber
    );

    if (userId <= 0)
        return new RegisterResponse { Success = false };

    // Log registration
    await _sprocedures.sp_maLogLoginHistoryAsync(
        userId: userId,
        ipAddress: "127.0.0.1",
        userAgent: "Registration",
        isSuccessful: true
    );

    return new RegisterResponse 
    { 
        Success = true, 
        UserId = userId,
        Email = request.Email
    };
}
```

#### Example 3: Update User Profile
```csharp
public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request)
{
    return await _sprocedures.sp_maUpdateUserAsync(
        userId: userId,
        firstName: request.FirstName,
        lastName: request.LastName,
        phoneNumber: request.PhoneNumber
    );
}
```

#### Example 4: Get All Users (Paginated)
```csharp
public async Task<(List<User> users, int total)> GetAllUsersAsync(int pageNumber, int pageSize)
{
    var (dataTable, totalCount) = await _sprocedures.sp_maGetAllUsersAsync(pageNumber, pageSize);
    
    var users = new List<User>();
    foreach (DataRow row in dataTable.Rows)
    {
        users.Add(new User
        {
            UserId = (int)row["UserId"],
            Username = row["Username"].ToString(),
            Email = row["Email"].ToString(),
            FirstName = row["FirstName"].ToString(),
            LastName = row["LastName"].ToString()
        });
    }
    
    return (users, totalCount);
}
```

#### Example 5: Assign Role to User
```csharp
public async Task<bool> AssignRoleAsync(int userId, int roleId, int adminId)
{
    return await _sprocedures.sp_maAssignRoleToUserAsync(
        userId: userId,
        roleId: roleId,
        assignedBy: adminId
    );
}
```

#### Example 6: Link External Provider
```csharp
public async Task<bool> LinkGoogleAsync(int userId, GoogleAuthResult googleAuth)
{
    return await _sprocedures.sp_maAddExternalAuthAsync(
        userId: userId,
        providerId: 1, // Google
        externalUserId: googleAuth.GoogleId,
        externalEmail: googleAuth.Email,
        accessToken: googleAuth.AccessToken
    );
}
```

#### Example 7: Soft Delete User (GDPR)
```csharp
public async Task<bool> DeleteAccountAsync(int userId)
{
    return await _sprocedures.sp_maSoftDeleteUserAsync(userId);
}
```

#### Example 8: Password Reset
```csharp
public async Task<string> CreatePasswordResetTokenAsync(int userId)
{
    var token = Guid.NewGuid().ToString("N");
    var expiresAt = DateTime.UtcNow.AddHours(2);
    
    return await _sprocedures.sp_maCreatePasswordResetTokenAsync(
        userId: userId,
        token: token,
        expiresAt: expiresAt
    );
}

public async Task<bool> ResetPasswordAsync(int userId, string token, string newPassword)
{
    var passwordHash = HashPassword(newPassword);
    
    return await _sprocedures.sp_maResetPasswordAsync(
        userId: userId,
        token: token,
        newPasswordHash: passwordHash
    );
}
```

#### Example 9: Verify Email
```csharp
public async Task<bool> VerifyEmailAsync(int userId)
{
    return await _sprocedures.sp_maVerifyEmailAsync(userId);
}
```

#### Example 10: Get User Profile with Roles
```csharp
public async Task<UserProfileResponse> GetProfileWithRolesAsync(int userId)
{
    var result = await _sprocedures.sp_maGetUserWithRolesAsync(userId);
    
    if (result.Rows.Count == 0)
        return null;
    
    var row = result.Rows[0];
    return new UserProfileResponse
    {
        UserId = (int)row["UserId"],
        Username = row["Username"].ToString(),
        Email = row["Email"].ToString(),
        FirstName = row["FirstName"].ToString(),
        LastName = row["LastName"].ToString(),
        IsEmailVerified = (bool)row["IsEmailVerified"],
        Roles = await GetRolesAsync(userId)
    };
}

private async Task<List<RoleInfo>> GetRolesAsync(int userId)
{
    var result = await _sprocedures.sp_maGetUserRolesAsync(userId);
    var roles = new List<RoleInfo>();
    
    foreach (DataRow row in result.Rows)
    {
        roles.Add(new RoleInfo
        {
            RoleId = (int)row["RoleId"],
            RoleName = row["RoleName"].ToString(),
            Description = row["Description"].ToString()
        });
    }
    
    return roles;
}
```

---

## 📋 Complete Method Reference

### User Management

#### sp_maLoginUserAsync
```csharp
Task<DataTable> sp_maLoginUserAsync(
    string email, 
    string passwordHash, 
    string ipAddress, 
    string userAgent)
```
**Purpose:** Authenticate user  
**Returns:** User record if valid credentials

#### sp_maInsertUserAsync
```csharp
Task<int> sp_maInsertUserAsync(
    string username,
    string email,
    string passwordHash,
    string firstName,
    string lastName,
    string phoneNumber)
```
**Purpose:** Create new user  
**Returns:** UserId of created user

#### sp_maUpdateUserAsync
```csharp
Task<bool> sp_maUpdateUserAsync(
    int userId,
    string firstName,
    string lastName,
    string phoneNumber,
    DateTime? updatedAt = null)
```
**Purpose:** Update user profile

#### sp_maGetUserByIdAsync
```csharp
Task<DataTable> sp_maGetUserByIdAsync(int userId)
```
**Purpose:** Get user by ID

#### sp_maGetUserByEmailAsync
```csharp
Task<DataTable> sp_maGetUserByEmailAsync(string email)
```
**Purpose:** Get user by email

#### sp_maGetAllUsersAsync
```csharp
Task<(DataTable users, int totalCount)> sp_maGetAllUsersAsync(
    int pageNumber, 
    int pageSize)
```
**Purpose:** Get paginated user list

#### sp_maSoftDeleteUserAsync
```csharp
Task<bool> sp_maSoftDeleteUserAsync(int userId)
```
**Purpose:** Soft delete user (GDPR compliant)

### Role Management

#### sp_maGetUserRolesAsync
```csharp
Task<DataTable> sp_maGetUserRolesAsync(int userId)
```
**Purpose:** Get all roles for user

#### sp_maAssignRoleToUserAsync
```csharp
Task<bool> sp_maAssignRoleToUserAsync(
    int userId, 
    int roleId, 
    int assignedBy)
```
**Purpose:** Assign role to user

#### sp_maGetUserWithRolesAsync
```csharp
Task<DataTable> sp_maGetUserWithRolesAsync(int userId)
```
**Purpose:** Get user with all roles and external auth

### External Providers

#### sp_maAddExternalAuthAsync
```csharp
Task<bool> sp_maAddExternalAuthAsync(
    int userId,
    int providerId,      // 1=Google, 2=Facebook, 3=GitHub
    string externalUserId,
    string externalEmail,
    string accessToken)
```
**Purpose:** Link external provider

### Authentication History

#### sp_maLogLoginHistoryAsync
```csharp
Task<bool> sp_maLogLoginHistoryAsync(
    int userId,
    string ipAddress,
    string userAgent,
    bool isSuccessful)
```
**Purpose:** Log login attempt

### Password Reset

#### sp_maCreatePasswordResetTokenAsync
```csharp
Task<string> sp_maCreatePasswordResetTokenAsync(
    int userId,
    string token,
    DateTime expiresAt)
```
**Purpose:** Create password reset token

#### sp_maResetPasswordAsync
```csharp
Task<bool> sp_maResetPasswordAsync(
    int userId,
    string token,
    string newPasswordHash)
```
**Purpose:** Reset password with token validation

#### sp_maVerifyEmailAsync
```csharp
Task<bool> sp_maVerifyEmailAsync(int userId)
```
**Purpose:** Mark email as verified

---

## ⚠️ Error Handling

All methods include comprehensive try-catch blocks and logging:

```csharp
try
{
    _logger.LogInformation("Executing {Method}", methodName);
    // Execute SP
    _logger.LogInformation("{Method} executed successfully", methodName);
    return result;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error executing {Method}", methodName);
    throw;
}
```

### Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| "Connection string not found" | Missing DefaultConnection in appsettings.json | Add connection string to config |
| "Invalid column name" | SP parameter name mismatch | Check parameter names in stored procedure |
| "Timeout expired" | Database taking too long | Increase CommandTimeout or optimize SP |
| "Database connection failed" | Server offline or wrong credentials | Verify server and connection string |

---

## 🔐 Security Best Practices

1. **Always hash passwords before passing to SP:**
   ```csharp
   var hash = HashPassword(plainPassword);
   await _sprocedures.sp_maInsertUserAsync(..., passwordHash: hash, ...);
   ```

2. **Use parameterized queries (built-in):**
   - All methods use `command.Parameters.AddWithValue()` to prevent SQL injection

3. **Handle sensitive data:**
   - Never log passwords or tokens
   - Use encrypted connection strings
   - Implement rate limiting on login attempts

4. **Validate before passing to SP:**
   ```csharp
   if (!ValidateEmail(email))
       throw new ArgumentException("Invalid email");
   
   var result = await _sprocedures.sp_maLoginUserAsync(email, ...);
   ```

---

## 📊 Performance Notes

- **Singleton Registration:** StoredProcedures is registered as singleton for optimal performance
- **Connection Pooling:** SqlClient automatically pools connections
- **Async/Await:** All methods are async to prevent blocking threads
- **Logging:** Informational logs only on success; errors include full exception details

---

## 🧪 Testing

### Unit Test Example
```csharp
[TestClass]
public class StoredProceduresTests
{
    private IStoredProcedures _sprocedures;
    private ILogger<StoredProcedures> _logger;

    [TestInitialize]
    public void Setup()
    {
        _logger = new Mock<ILogger<StoredProcedures>>().Object;
        var connectionString = "Server=DESKTOP-O4ATQ75\\MSSQLSERVER2;Database=Manam;Integrated Security=true;";
        _sprocedures = new StoredProcedures(connectionString, _logger);
    }

    [TestMethod]
    public async Task RegisterUser_Success()
    {
        var userId = await _sprocedures.sp_maInsertUserAsync(
            "testuser",
            "test@example.com",
            "hash123",
            "Test",
            "User",
            "+1-555-0000"
        );

        Assert.IsTrue(userId > 0);
    }

    [TestMethod]
    public async Task LoginUser_ValidCredentials()
    {
        var result = await _sprocedures.sp_maLoginUserAsync(
            "test@example.com",
            "hash123",
            "127.0.0.1",
            "TestAgent/1.0"
        );

        Assert.IsTrue(result.Rows.Count > 0);
    }
}
```

---

## 📞 Support

- **Logs:** Check application logs for detailed error information
- **Database:** Query `maUserLoginHistory` for login audit trail
- **Connection:** Verify connection string format: `Server=HOST\INSTANCE;Database=DB;Integrated Security=true;`

---

**Version:** 1.0  
**Created:** 2025-01-15  
**Status:** ✅ Production Ready  
**Database:** DESKTOP-O4ATQ75\MSSQLSERVER2\Manam  
**Sync Status:** ✓ All 15 SPs synchronized  
**Sync Status:** ✓ All 7 tables synchronized
