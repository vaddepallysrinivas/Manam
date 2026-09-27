
╔════════════════════════════════════════════════════════════════════════════╗
║                     MANAM DATABASE SETUP - VISUAL GUIDE                    ║
╚════════════════════════════════════════════════════════════════════════════╝


┌──────────────────────────────────────────────────────────────────────────┐
│ QUICK START - CHOOSE YOUR PATH                                           │
└──────────────────────────────────────────────────────────────────────────┘

	┌─────────────────┐      ┌──────────────────┐      ┌──────────────────┐
	│  GUI Method     │      │  PowerShell      │      │  Command Line    │
	│  (SSMS)         │      │  (Automated)     │      │  (Manual)        │
	├─────────────────┤      ├──────────────────┤      ├──────────────────┤
	│ 1. Open SSMS    │      │ 1. Open PS       │      │ 1. Open PS       │
	│ 2. Connect      │      │ 2. cd Database   │      │ 2. Run command   │
	│ 3. Open file    │      │ 3. Run script    │      │ 3. Watch output  │
	│ 4. Press F5     │      │ 4. Done!         │      │ 4. Done!         │
	│ ∼ 1 minute      │      │ ∼ 30 seconds     │      │ ∼ 30 seconds     │
	└─────────────────┘      └──────────────────┘      └──────────────────┘
		 Easiest                  Fastest                  Simple


┌──────────────────────────────────────────────────────────────────────────┐
│ EXECUTION FLOW                                                           │
└──────────────────────────────────────────────────────────────────────────┘

			  ┌────────────────────────────────────┐
			  │  01_CreateDatabase.sql ⭐ REQUIRED │
			  │  • Create database                 │
			  │  • Create Users table              │
			  │  • Create 9 stored procedures      │
			  │  • Create 3 indexes                │
			  │  ∼ 5 seconds                       │
			  └────────────────────────────────────┘
							↓
			  ┌────────────────────────────────────┐
			  │  02_VerifyDatabase.sql ✅ VERIFY   │
			  │  • Check all objects created       │
			  │  • List tables, columns, procs     │
			  │  ∼ 2 seconds                       │
			  └────────────────────────────────────┘
							↓
			  ┌────────────────────────────────────┐
			  │  03_InsertTestData.sql ⚪ OPTIONAL │
			  │  • Add 3 test users                │
			  │  • Test CRUD operations            │
			  │  ∼ 2 seconds                       │
			  └────────────────────────────────────┘
							↓
			  ┌────────────────────────────────────┐
			  │  04_TestConnection.sql ⚪ OPTIONAL │
			  │  • Test database access            │
			  │  • Validate procedures             │
			  │  ∼ 1 second                        │
			  └────────────────────────────────────┘
							↓
				  ✅ READY TO USE!


┌──────────────────────────────────────────────────────────────────────────┐
│ DATABASE STRUCTURE                                                       │
└──────────────────────────────────────────────────────────────────────────┘

	┌──────────────────────────────────────────────┐
	│           MANAM DATABASE                     │
	│  Server: DESKTOP-O4ATQ75\MSSQLSERVER2       │
	├──────────────────────────────────────────────┤
	│                                              │
	│  ┌────────────────────────────────────────┐ │
	│  │         USERS TABLE (1)                │ │
	│  ├────────────────────────────────────────┤ │
	│  │ ├─ Id (PK, GUID)                       │ │
	│  │ ├─ Username (UNIQUE, INDEXED)          │ │
	│  │ ├─ Email (UNIQUE, INDEXED)             │ │
	│  │ ├─ PasswordHash                        │ │
	│  │ ├─ FirstName                           │ │
	│  │ ├─ LastName                            │ │
	│  │ ├─ IsActive (INDEXED, Default=1)       │ │
	│  │ ├─ FailedLoginAttempts (Default=0)     │ │
	│  │ ├─ LockoutUntil                        │ │
	│  │ ├─ CreatedAt                           │ │
	│  │ └─ UpdatedAt                           │ │
	│  └────────────────────────────────────────┘ │
	│                                              │
	│  ┌────────────────────────────────────────┐ │
	│  │   STORED PROCEDURES (9)                │ │
	│  ├────────────────────────────────────────┤ │
	│  │ ├─ sp_GetUserById                      │ │
	│  │ ├─ sp_GetUserByUsername                │ │
	│  │ ├─ sp_GetUserByEmail                   │ │
	│  │ ├─ sp_GetAllUsers                      │ │
	│  │ ├─ sp_CreateUser                       │ │
	│  │ ├─ sp_UpdateUser                       │ │
	│  │ ├─ sp_DeleteUser                       │ │
	│  │ ├─ sp_UpdateUserLockout                │ │
	│  │ └─ sp_ResetFailedLoginAttempts         │ │
	│  └────────────────────────────────────────┘ │
	│                                              │
	│  ┌────────────────────────────────────────┐ │
	│  │      INDEXES (3)                       │ │
	│  ├────────────────────────────────────────┤ │
	│  │ ├─ PK_Users (ID)                       │ │
	│  │ ├─ IX_Users_Username                   │ │
	│  │ └─ IX_Users_IsActive                   │ │
	│  └────────────────────────────────────────┘ │
	│                                              │
	└──────────────────────────────────────────────┘


┌──────────────────────────────────────────────────────────────────────────┐
│ FILE ORGANIZATION                                                        │
└──────────────────────────────────────────────────────────────────────────┘

	Database Folder/
	│
	├── 📋 SQL SCRIPTS
	│   ├── 01_CreateDatabase.sql ⭐ START HERE
	│   ├── 02_VerifyDatabase.sql ✅ THEN THIS
	│   ├── 03_InsertTestData.sql ⚪ OPTIONAL
	│   └── 04_TestConnection.sql ⚪ OPTIONAL
	│
	├── 🔧 AUTOMATION
	│   └── SetupDatabase.ps1 (PowerShell helper)
	│
	└── 📚 DOCUMENTATION
		├── INDEX.md ⭐ READ THIS FIRST
		├── README.md (Overview)
		├── QUICK_REFERENCE.md (Quick lookup)
		├── DATABASE_SETUP_GUIDE.md (Complete guide)
		└── DATABASE_CREATION_SUMMARY.md (Full details)


┌──────────────────────────────────────────────────────────────────────────┐
│ CONNECTION STRING                                                        │
└──────────────────────────────────────────────────────────────────────────┘

	Server: DESKTOP-O4ATQ75\MSSQLSERVER2
	Database: ManamDB
	Authentication: Windows (Integrated Security)

	✅ Already configured in: Manam.API/appsettings.json
	✅ No changes needed!


┌──────────────────────────────────────────────────────────────────────────┐
│ TIMELINE                                                                 │
└──────────────────────────────────────────────────────────────────────────┘

	01_CreateDatabase.sql    [====        ] 5 seconds
	02_VerifyDatabase.sql    [==          ] 2 seconds
	03_InsertTestData.sql    [==          ] 2 seconds (Optional)
	04_TestConnection.sql    [=           ] 1 second (Optional)
	━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	TOTAL (Essential)        [======      ] ∼ 7 seconds
	TOTAL (All Optional)     [========    ] ∼ 10 seconds


┌──────────────────────────────────────────────────────────────────────────┐
│ VERIFICATION STEPS                                                       │
└──────────────────────────────────────────────────────────────────────────┘

	Step 1: Run 01_CreateDatabase.sql
			↓
			See message: "Manam Database Setup Complete!"
			↓
	Step 2: Run 02_VerifyDatabase.sql
			↓
			Verify output shows:
			• Database: ManamDB ✓
			• Table: Users ✓
			• 11 Columns ✓
			• 9 Procedures ✓
			• 3 Indexes ✓
			↓
	Step 3: ✅ Database Ready!


┌──────────────────────────────────────────────────────────────────────────┐
│ TROUBLESHOOTING FLOWCHART                                                │
└──────────────────────────────────────────────────────────────────────────┘

						Script Failed?
							 ↓
					┌────────────────────┐
					│ Check Error Message │
					└────────────────────┘
							 ↓
		┌────────────────────┬─────────────────────┐
		↓                    ↓                     ↓
	"Already Exists"    "Timeout"          "Connection Failed"
		↓                    ↓                     ↓
	Drop database        Increase timeout     Check SQL Server
		↓                    ↓                     ↓
	Re-run script       Re-run script         Verify instance
		↓                    ↓                     ↓
	✅ Done           ✅ Done                Verify auth
												  ↓
											  ✅ Done


┌──────────────────────────────────────────────────────────────────────────┐
│ KEY TAKEAWAYS                                                            │
└──────────────────────────────────────────────────────────────────────────┘

	✅ Everything is automated - just execute the scripts
	✅ Connection string already configured
	✅ No manual SQL needed - all scripts provided
	✅ Safe to run multiple times
	✅ Complete in ~10 seconds
	✅ documentation provided for everything


┌──────────────────────────────────────────────────────────────────────────┐
│ START HERE!                                                              │
└──────────────────────────────────────────────────────────────────────────┘

	1️⃣  Open: 01_CreateDatabase.sql
	2️⃣  Execute Script (F5 in SSMS)
	3️⃣  Wait for: "Manam Database Setup Complete!"
	4️⃣  Done! ✅

	OR

	1️⃣  Open PowerShell
	2️⃣  cd Database
	3️⃣  .\SetupDatabase.ps1 -Action All
	4️⃣  Done! ✅


╔════════════════════════════════════════════════════════════════════════════╗
║                  Ready to create your database? Start now! 🚀              ║
╚════════════════════════════════════════════════════════════════════════════╝
