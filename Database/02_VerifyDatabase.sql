-- ============================================================
-- Verification Queries - Run After Database Creation
-- ============================================================
-- Purpose: Verify all database objects were created successfully
-- ============================================================

USE ManamDB;
GO

PRINT '========== DATABASE VERIFICATION ==========';
PRINT '';

-- 1. Check Database
PRINT '1. Database Status:';
SELECT 
	name AS [DatabaseName],
	state_desc AS [Status],
	recovery_model_desc AS [RecoveryModel]
FROM sys.databases 
WHERE name = 'ManamDB';

PRINT '';

-- 2. Check Tables
PRINT '2. Tables Created:';
SELECT 
	TABLE_NAME AS [TableName],
	TABLE_TYPE AS [Type]
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo';

PRINT '';

-- 3. Check Users Table Structure
PRINT '3. Users Table Columns:';
SELECT 
	COLUMN_NAME AS [ColumnName],
	DATA_TYPE AS [DataType],
	IS_NULLABLE AS [Nullable],
	COLUMN_DEFAULT AS [DefaultValue]
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;

PRINT '';

-- 4. Check Stored Procedures
PRINT '4. Stored Procedures Created:';
SELECT 
	name AS [ProcedureName],
	type_desc AS [Type],
	created AS [CreatedDate],
	modified AS [ModifiedDate]
FROM sys.objects
WHERE type = 'P' 
AND schema_id = SCHEMA_ID('dbo')
ORDER BY name;

PRINT '';

-- 5. Check Indexes
PRINT '5. Indexes on Users Table:';
SELECT 
	i.name AS [IndexName],
	CASE WHEN i.is_primary_key = 1 THEN 'PRIMARY KEY' 
		 WHEN i.is_unique = 1 THEN 'UNIQUE'
		 ELSE 'NON-UNIQUE' END AS [IndexType],
	c.name AS [ColumnName]
FROM sys.indexes i
JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('dbo.Users')
ORDER BY i.name, ic.key_ordinal;

PRINT '';
PRINT '========== VERIFICATION COMPLETE ==========';
PRINT '';
PRINT 'All tables, procedures, and indexes should be visible above.';
PRINT 'If any are missing, re-run 01_CreateDatabase.sql';

GO
