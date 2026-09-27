# 📊 Database Quick Reference Card

## 🎯 TL;DR - Get Database Running in 2 Minutes

### Option 1: GUI (Easiest)
```
1. Open SQL Server Management Studio (SSMS)
2. Connect to: DESKTOP-O4ATQ75\MSSQLSERVER2
3. File → Open → File → Select "01_CreateDatabase.sql"
4. Click: Execute (or press F5)
5. Wait: ~5 seconds for completion
```

### Option 2: PowerShell (Fastest)
```powershell
cd C:\Users\srini\source\Repos\Manam\Database
.\SetupDatabase.ps1 -Action All
```

### Option 3: Command Line (Manual)
```powershell
sqlcmd -S "DESKTOP-O4ATQ75\MSSQLSERVER2" -i "Database\01_CreateDatabase.sql"
```

---

## ✅ What's Included

| File | Size | Purpose |
|------|------|---------|
| `01_CreateDatabase.sql` | ~9KB | Create DB + Tables + Procedures |
| `02_VerifyDatabase.sql` | ~2KB | Verify everything was created |
| `03_InsertTestData.sql` | ~2KB | Insert 3 test users |
| `SetupDatabase.ps1` | ~4KB | PowerShell automation |
| `DATABASE_SETUP_GUIDE.md` | ~10KB | Comprehensive guide |
| `README.md` | ~8KB | This folder overview |

---

## 📋 Database Structure

```
Database: ManamDB
│
├─ Table: Users
│  ├─ Id (GUID, PK)
│  ├─ Username (Unique, Indexed)
│  ├─ Email (Unique, Indexed)
│  ├─ PasswordHash
│  ├─ FirstName
│  ├─ LastName
│  ├─ IsActive (Indexed)
│  ├─ FailedLoginAttempts
│  ├─ LockoutUntil
│  ├─ CreatedAt
│  └─ UpdatedAt
│
├─ Procedures (9):
│  ├─ sp_GetUserById
│  ├─ sp_GetUserByUsername
│  ├─ sp_GetUserByEmail
│  ├─ sp_GetAllUsers
│  ├─ sp_CreateUser
│  ├─ sp_UpdateUser
│  ├─ sp_DeleteUser
│  ├─ sp_UpdateUserLockout
│  └─ sp_ResetFailedLoginAttempts
│
└─ Indexes (3):
   ├─ IX_Users_Username
   ├─ IX_Users_Email
   └─ IX_Users_IsActive
```

---

## 🔗 Connection Info

**Server**: `DESKTOP-O4ATQ75\MSSQLSERVER2`  
**Database**: `ManamDB`  
**Authentication**: Windows (Integrated Security)

### Connection String
```
Server=DESKTOP-O4ATQ75\MSSQLSERVER2;Database=ManamDB;Integrated Security=true;TrustServerCertificate=true;
```

### For appsettings.json
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=DESKTOP-O4ATQ75\\MSSQLSERVER2;Database=ManamDB;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```
✅ Already configured - no changes needed!

---

## 🚀 Execution Steps

### Step 1: Create Database
```sql
-- Execute: 01_CreateDatabase.sql
-- Time: ~5 seconds
-- Result: ✅ Database + Tables + Procedures created
```

### Step 2: Verify Creation
```sql
-- Execute: 02_VerifyDatabase.sql
-- Time: ~2 seconds
-- Result: ✅ Lists all objects created
```

### Step 3: (Optional) Add Test Data
```sql
-- Execute: 03_InsertTestData.sql
-- Time: ~2 seconds
-- Result: ✅ 3 test users inserted
```

---

## 🧪 Stored Procedures

### Get Operations
| Procedure | Parameter | Returns |
|-----------|-----------|---------|
| `sp_GetUserById` | @Id | Single user |
| `sp_GetUserByUsername` | @Username | Single user |
| `sp_GetUserByEmail` | @Email | Single user |
| `sp_GetAllUsers` | None | All users |

### Write Operations
| Procedure | Parameters | Returns |
|-----------|------------|---------|
| `sp_CreateUser` | @Id, @Username, @Email, @PasswordHash, @FirstName, @LastName, @IsActive | UserId |
| `sp_UpdateUser` | @Id, @Username, @Email, @PasswordHash, @FirstName, @LastName, @IsActive | RowsAffected |
| `sp_DeleteUser` | @Id | RowsAffected |
| `sp_UpdateUserLockout` | @Id, @LockoutUntil | RowsAffected |
| `sp_ResetFailedLoginAttempts` | @Id | RowsAffected |

---

## ✅ Verification Queries

Copy and paste these to verify database:

```sql
-- 1. Database exists
SELECT name FROM sys.databases WHERE name = 'ManamDB';

-- 2. Users table exists
SELECT * FROM ManamDB.INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'Users';

-- 3. Table columns
SELECT COLUMN_NAME, DATA_TYPE 
FROM ManamDB.INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;

-- 4. Stored procedures
SELECT name FROM sys.objects 
WHERE type = 'P' AND db_id() = DB_ID('ManamDB')
ORDER BY name;

-- 5. Indexes
SELECT name FROM sys.indexes 
WHERE object_id = OBJECT_ID('ManamDB.dbo.Users')
AND name LIKE 'IX_%';
```

---

## 🐛 Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| "Database already exists" | Script auto-drops old DB |
| "Procedure already exists" | Drop procedures manually |
| "Cannot connect" | Check SQL Server is running |
| "Timeout expired" | Increase timeout in SSMS |
| "sqlcmd not found" | Install SQL Server tools |

---

## 📚 File Reference

| Need | See |
|------|-----|
| Step-by-step setup | `DATABASE_SETUP_GUIDE.md` |
| Complete overview | `README.md` |
| Auto-execute scripts | `SetupDatabase.ps1` |
| Database creation | `01_CreateDatabase.sql` |
| Verify objects | `02_VerifyDatabase.sql` |
| Test data | `03_InsertTestData.sql` |

---

## 🎯 Common Tasks

### Test Database Connection
```csharp
using System.Data.SqlClient;

var connectionString = "Server=DESKTOP-O4ATQ75\\MSSQLSERVER2;Database=ManamDB;Integrated Security=true;";
using (var connection = new SqlConnection(connectionString))
{
	connection.Open();
	Console.WriteLine("✅ Connected!");
}
```

### Get All Users
```sql
EXEC sp_GetAllUsers;
```

### Create Test User
```sql
EXEC sp_CreateUser
	@Id = NEWID(),
	@Username = 'testuser',
	@Email = 'test@example.com',
	@PasswordHash = 'hashed_password',
	@FirstName = 'Test',
	@LastName = 'User',
	@IsActive = 1;
```

### Update User
```sql
EXEC sp_UpdateUser
	@Id = 'user-id-here',
	@Username = 'newusername',
	@Email = 'newemail@example.com',
	@PasswordHash = 'new_hash',
	@FirstName = 'New',
	@LastName = 'Name',
	@IsActive = 1;
```

---

## ⏱️ Timeline

| Task | Time |
|------|------|
| Execute 01_CreateDatabase.sql | ~5 sec |
| Execute 02_VerifyDatabase.sql | ~2 sec |
| Execute 03_InsertTestData.sql | ~2 sec |
| **Total** | **~9 seconds** |

---

## 🎉 Success Indicators

After execution, you should see:

✅ Database `ManamDB` created  
✅ Table `Users` with 11 columns  
✅ 9 Stored procedures created  
✅ 3 Non-clustered indexes created  
✅ All scripts complete without errors  
✅ Application can connect and execute queries

---

## 📞 Need Help?

1. Check **DATABASE_SETUP_GUIDE.md** → Troubleshooting section
2. Run **02_VerifyDatabase.sql** → See what was created
3. Verify connection → Try connection string in SSMS
4. Check SQL Server → Verify SQL Server is running

---

## 🚀 Next Steps After Setup

1. ✅ Execute database scripts
2. ✅ Verify database created
3. 🔜 Start Manam.API
4. 🔜 Test endpoints
5. 🔜 Insert production data

---

**Status**: Ready to execute!  
**Time to Complete**: ~2 minutes  
**Difficulty**: Easy ⭐  

**Start with: `01_CreateDatabase.sql`** 🎉
