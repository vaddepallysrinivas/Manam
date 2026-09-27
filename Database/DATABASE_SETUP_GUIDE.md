# 📊 Manam Database Setup Guide

## 🎯 Quick Start - 3 Steps

### Step 1: Execute SQL Script
1. Open **SQL Server Management Studio (SSMS)**
2. Connect to: `DESKTOP-O4ATQ75\MSSQLSERVER2`
3. Open file: `Database/01_CreateDatabase.sql`
4. Click **Execute** (or press **F5**)
5. Wait for completion message

### Step 2: Verify Database Created
```sql
-- Run this to verify:
SELECT name FROM sys.databases WHERE name = 'ManamDB';

-- Should return: ManamDB
```

### Step 3: Verify Stored Procedures
```sql
-- Run this to verify all procedures were created:
SELECT name FROM sys.objects 
WHERE type = 'P' AND schema_id = 1 
ORDER BY name;

-- Should show 9 procedures (sp_GetUserById, sp_CreateUser, etc.)
```

---

## 🔧 Or Manual Step-by-Step (If Preferred)

### Option A: Query Editor Method (Recommended)

**1. Connect to SQL Server:**
```
Server: DESKTOP-O4ATQ75\MSSQLSERVER2
Authentication: Windows Authentication
```

**2. Create Database:**
```sql
-- Copy the CREATE DATABASE section from 01_CreateDatabase.sql
-- Paste into new query window
-- Execute
```

**3. Create Tables:**
```sql
-- Copy the CREATE TABLE section
-- Execute
```

**4. Create Stored Procedures:**
```sql
-- Copy each CREATE PROCEDURE section
-- Execute each one
```

### Option B: Command Line Method

**Open PowerShell and run:**

```powershell
# Set variables
$ServerName = "DESKTOP-O4ATQ75\MSSQLSERVER2"
$ScriptPath = "C:\Users\srini\source\Repos\Manam\Database\01_CreateDatabase.sql"

# Execute script
sqlcmd -S $ServerName -i $ScriptPath
```

---

## 📋 What Gets Created

### Database
- **Name**: `ManamDB`
- **Location**: Default (your SQL Server default data path)

### Tables
- **Users** table with columns:
  - Id (GUID, Primary Key)
  - Username (unique, indexed)
  - Email (unique, indexed)
  - PasswordHash
  - FirstName
  - LastName
  - IsActive (indexed)
  - FailedLoginAttempts
  - LockoutUntil
  - CreatedAt
  - UpdatedAt

### Stored Procedures (9 total)
| Procedure | Purpose |
|-----------|---------|
| `sp_GetUserById` | Get user by ID |
| `sp_GetUserByUsername` | Get user by username |
| `sp_GetUserByEmail` | Get user by email |
| `sp_GetAllUsers` | Get all users |
| `sp_CreateUser` | Create new user |
| `sp_UpdateUser` | Update user |
| `sp_DeleteUser` | Delete user |
| `sp_UpdateUserLockout` | Update lockout timestamp |
| `sp_ResetFailedLoginAttempts` | Reset failed attempts |

### Indexes
- IX_Users_Username
- IX_Users_Email
- IX_Users_IsActive

---

## ✅ Verification Checklist

After execution, verify everything:

```sql
-- 1. Check database exists
SELECT name FROM sys.databases WHERE name = 'ManamDB';

-- 2. Check Users table exists
SELECT * FROM ManamDB.INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'Users';

-- 3. Check columns
SELECT COLUMN_NAME, DATA_TYPE 
FROM ManamDB.INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;

-- 4. Check stored procedures
SELECT name FROM sys.objects 
WHERE type = 'P' AND db_id() = (SELECT database_id FROM sys.databases WHERE name = 'ManamDB')
ORDER BY name;

-- 5. Check indexes
SELECT name FROM sys.indexes 
WHERE object_id = OBJECT_ID('ManamDB.dbo.Users')
AND name LIKE 'IX_%';
```

---

## 🔗 Connection Strings

### For appsettings.json (Already Correct)
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=DESKTOP-O4ATQ75\\MSSQLSERVER2;Database=ManamDB;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

### Alternative Connection Strings

**Using localhost:**
```
Server=localhost\MSSQLSERVER2;Database=ManamDB;Integrated Security=true;TrustServerCertificate=true;
```

**With SQL Authentication (if needed):**
```
Server=DESKTOP-O4ATQ75\MSSQLSERVER2;Database=ManamDB;User Id=sa;Password=YourPassword;
```

**Short form:**
```
Server=(local)\MSSQLSERVER2;Database=ManamDB;Integrated Security=true;
```

---

## 🧪 Test Connection from .NET

```csharp
// Add this to Program.cs temporarily to test
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
using (var connection = new SqlConnection(connectionString))
{
	try
	{
		connection.Open();
		Console.WriteLine("✅ Database connection successful!");
	}
	catch (Exception ex)
	{
		Console.WriteLine($"❌ Connection failed: {ex.Message}");
	}
}
```

---

## 🚀 Test Data (Optional)

After database is created, you can insert test data:

```sql
USE ManamDB;

-- Insert test user
DECLARE @UserId UNIQUEIDENTIFIER = NEWID();

EXEC sp_CreateUser
	@Id = @UserId,
	@Username = 'testuser',
	@Email = 'testuser@example.com',
	@PasswordHash = 'hashed_password_here',
	@FirstName = 'Test',
	@LastName = 'User',
	@IsActive = 1;

-- Retrieve the test user
EXEC sp_GetUserByUsername @Username = 'testuser';
```

---

## 🐛 Troubleshooting

### Error: "Cannot connect to SQL Server"
- ✅ Verify SQL Server is running
- ✅ Verify instance name: `MSSQLSERVER2`
- ✅ Check Windows Authentication is enabled

### Error: "Database already exists"
- ✅ The script drops existing database automatically
- ✅ If you get an error, run in SQLCMD mode or increase timeout

### Error: "Procedure already exists"
- ✅ Drop procedures first:
```sql
DROP PROCEDURE IF EXISTS dbo.sp_GetUserById;
DROP PROCEDURE IF EXISTS dbo.sp_GetUserByUsername;
-- etc...
```

### Error: "Index already exists"
- ✅ The script uses `CREATE NONCLUSTERED INDEX` which fails if index exists
- ✅ Change to: `CREATE NONCLUSTERED INDEX IF NOT EXISTS`

---

## 📊 Database Diagram

```
┌────────────────────────────────────────┐
│            Users Table                 │
├────────────────────────────────────────┤
│ Id (PK, GUID)                          │
│ Username (UNIQUE, INDEXED)             │
│ Email (UNIQUE, INDEXED)                │
│ PasswordHash (NVARCHAR(MAX))           │
│ FirstName (NVARCHAR(100))              │
│ LastName (NVARCHAR(100))               │
│ IsActive (BIT, INDEXED, Default=1)     │
│ FailedLoginAttempts (INT, Default=0)   │
│ LockoutUntil (DATETIME2, NULL)         │
│ CreatedAt (DATETIME2)                  │
│ UpdatedAt (DATETIME2)                  │
└────────────────────────────────────────┘

Stored Procedures: 9 total
Indexes: 3 total
```

---

## 📚 Next Steps

1. ✅ Execute the SQL script (step 1 above)
2. ✅ Verify all objects created
3. ✅ Test connection from .NET application
4. ✅ (Optional) Insert test data
5. ✅ Run your Manam API application

---

## 💡 Quick Command Reference

```powershell
# Connect to SQL Server via command line
sqlcmd -S DESKTOP-O4ATQ75\MSSQLSERVER2

# Execute script from PowerShell
sqlcmd -S DESKTOP-O4ATQ75\MSSQLSERVER2 -i "Database\01_CreateDatabase.sql"

# Connect with specific credentials
sqlcmd -S DESKTOP-O4ATQ75\MSSQLSERVER2 -U sa -P YourPassword
```

---

**Status**: ✅ Ready to execute  
**File**: `Database/01_CreateDatabase.sql`  
**Target**: `DESKTOP-O4ATQ75\MSSQLSERVER2`  
**Database**: `ManamDB`

Execute the SQL script and you're done! 🎉
