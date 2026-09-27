# 🗄️ DATABASE SETUP - START HERE

Welcome! This folder contains everything needed to create your Manam database.

---

## ⚡ QUICK START (Pick One)

### 🖥️ Using SQL Server Management Studio (GUI)
```
1. Open SQL Server Management Studio
2. Connect to: DESKTOP-O4ATQ75\MSSQLSERVER2
3. File → Open → 01_CreateDatabase.sql
4. Press F5 to execute
5. Done! ✅
Time: 1 minute
```

### ⚙️ Using PowerShell (Recommended for automation)
```powershell
cd Database
.\SetupDatabase.ps1 -Action All
```
Time: 30 seconds

### 💻 Using Command Line
```powershell
sqlcmd -S "DESKTOP-O4ATQ75\MSSQLSERVER2" -i "01_CreateDatabase.sql"
```
Time: 30 seconds

---

## 📂 WHAT'S IN THIS FOLDER

| File | Purpose | Status |
|------|---------|--------|
| **01_CreateDatabase.sql** ⭐ | Create database & all objects | Ready |
| **02_VerifyDatabase.sql** | Verify everything was created | Ready |
| **03_InsertTestData.sql** | Add 3 test users | Optional |
| **04_TestConnection.sql** | Test database connection | Optional |
| **SetupDatabase.ps1** | PowerShell automation script | Ready |
| **README.md** | Detailed overview | Ready |
| **DATABASE_SETUP_GUIDE.md** | Complete setup guide | Ready |
| **QUICK_REFERENCE.md** | Quick lookup card | Ready |
| **DATABASE_CREATION_SUMMARY.md** | Complete information | Ready |
| **INDEX.md** | This file | Ready |

---

## 📝 DATABASES & OBJECTS CREATED

### Database
- **Name**: `ManamDB`
- **Server**: `DESKTOP-O4ATQ75\MSSQLSERVER2`
- **Size**: ~5 MB initial

### Tables (1)
- `Users` - 11 columns for user management

### Stored Procedures (9)
- `sp_GetUserById`
- `sp_GetUserByUsername`
- `sp_GetUserByEmail`
- `sp_GetAllUsers`
- `sp_CreateUser`
- `sp_UpdateUser`
- `sp_DeleteUser`
- `sp_UpdateUserLockout`
- `sp_ResetFailedLoginAttempts`

### Indexes (3)
- `IX_Users_Username`
- `IX_Users_Email`
- `IX_Users_IsActive`

---

## 🎯 EXECUTION FLOW

```
┌─────────────────────────────────────────────┐
│  01_CreateDatabase.sql (Execute First)      │
│  ├─ Drop old database                       │
│  ├─ Create ManamDB database                 │
│  ├─ Create Users table                      │
│  ├─ Create 9 stored procedures              │
│  └─ Create 3 indexes                        │
│  Time: ~5 seconds                           │
└─────────────────────────────────────────────┘
					↓
┌─────────────────────────────────────────────┐
│  02_VerifyDatabase.sql (Recommended Next)   │
│  ├─ Verify database exists                  │
│  ├─ Verify tables exist                     │
│  ├─ List all columns                        │
│  ├─ List all procedures                     │
│  └─ List all indexes                        │
│  Time: ~2 seconds                           │
└─────────────────────────────────────────────┘
					↓
		 (Optional from here)
					↓
┌─────────────────────────────────────────────┐
│  03_InsertTestData.sql (Add Test Users)     │
│  ├─ Create admin user                       │
│  ├─ Create john.doe user                    │
│  ├─ Create jane.smith user                  │
│  └─ Verify data inserted                    │
│  Time: ~2 seconds                           │
└─────────────────────────────────────────────┘
					↓
┌─────────────────────────────────────────────┐
│  04_TestConnection.sql (Test Connection)    │
│  ├─ Test database access                    │
│  ├─ Test stored procedures                  │
│  └─ Show connection info                    │
│  Time: ~1 second                            │
└─────────────────────────────────────────────┘
```

---

## 🔗 CONNECTION STRING

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=DESKTOP-O4ATQ75\\MSSQLSERVER2;Database=ManamDB;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

✅ **Already configured** in `Manam.API/appsettings.json`

---

## 📚 DOCUMENTATION GUIDE

| Need | Read This | Time |
|------|-----------|------|
| Just get it running | This file | 2 min |
| Step-by-step instructions | DATABASE_SETUP_GUIDE.md | 10 min |
| Quick lookup | QUICK_REFERENCE.md | 3 min |
| Complete info | DATABASE_CREATION_SUMMARY.md | 15 min |
| Folder overview | README.md | 5 min |

---

## ✅ VERIFICATION CHECKLIST

After running scripts, verify:

- [ ] 01_CreateDatabase.sql completes without errors
- [ ] 02_VerifyDatabase.sql shows all objects
- [ ] Users table has 11 columns
- [ ] All 9 stored procedures listed
- [ ] All 3 indexes listed
- [ ] (Optional) Test data shows 3 users

---

## 🧪 QUICK TEST

After database is created, run this query to verify:

```sql
-- Connect to DESKTOP-O4ATQ75\MSSQLSERVER2
USE ManamDB;
GO

-- This should return the names of all 9 procedures
SELECT name FROM sys.objects 
WHERE type = 'P' AND schema_id = 1
ORDER BY name;
```

Expected output (9 rows):
```
sp_CreateUser
sp_DeleteUser
sp_GetAllUsers
sp_GetUserByEmail
sp_GetUserById
sp_GetUserByUsername
sp_ResetFailedLoginAttempts
sp_UpdateUser
sp_UpdateUserLockout
```

---

## ⏱️ TIMELINE

| Task | Time |
|------|------|
| Execute 01_CreateDatabase.sql | 5 sec |
| Run 02_VerifyDatabase.sql | 2 sec |
| Optional: 03_InsertTestData.sql | 2 sec |
| Optional: 04_TestConnection.sql | 1 sec |
| **TOTAL** | **~10 seconds** |

---

## 🚀 NEXT STEPS

1. **NOW**: Execute `01_CreateDatabase.sql`
2. **AFTER**: Run `02_VerifyDatabase.sql` to verify
3. **OPTIONAL**: Run `03_InsertTestData.sql` for test data
4. **READY**: Start your Manam.API application!

---

## ⚠️ IMPORTANT NOTES

✅ **Connection string is already configured** in appsettings.json  
✅ **Windows Authentication** is used (no password needed)  
✅ **Database creation is automatic** - script handles everything  
✅ **Safe to run multiple times** - script drops old database first  

---

## 🐛 NEED HELP?

| Issue | Solution |
|-------|----------|
| Can't find the script? | You're in the right folder already! |
| SQL Server not running? | Start SQL Server Configuration Manager |
| Connection failed? | Verify: Server=DESKTOP-O4ATQ75\MSSQLSERVER2 |
| Script timeout? | Increase timeout in SSMS (Query Options) |
| Permission denied? | Run as Administrator, or use SQL Auth |

**More help**: See `DATABASE_SETUP_GUIDE.md` → Troubleshooting

---

## 📞 REFERENCE QUICK LINKS

- **Setup Steps**: DATABASE_SETUP_GUIDE.md
- **Quick Answers**: QUICK_REFERENCE.md
- **Full Overview**: DATABASE_CREATION_SUMMARY.md
- **Folder Info**: README.md

---

## 🎯 RIGHT NOW - DO THIS:

### Option 1: GUI (Easiest)
```
1. Open 01_CreateDatabase.sql (in this folder)
2. Open SQL Server Management Studio
3. File → Open → Choose 01_CreateDatabase.sql
4. Connect to: DESKTOP-O4ATQ75\MSSQLSERVER2
5. Press F5
```

### Option 2: PowerShell (Fastest)
```powershell
.\SetupDatabase.ps1 -Action All
```

---

## ✨ WHAT YOU GET

✅ Database: `ManamDB`  
✅ Table: `Users` (with 11 columns)  
✅ Procedures: 9 CRUD stored procedures  
✅ Indexes: 3 performance indexes  
✅ Ready to use: Immediately!  

---

## 🎉 YOU'RE ALL SET!

Everything is ready. Pick your execution method above and run it now!

**Total setup time: ~10 seconds**

---

**Status**: ✅ Ready to execute  
**Target**: DESKTOP-O4ATQ75\MSSQLSERVER2  
**Database**: ManamDB  
**Next**: Execute 01_CreateDatabase.sql!
