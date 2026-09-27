-- ============================================================
-- Test Connection and Validate Configuration
-- ============================================================
-- Purpose: Test connection string and validate database setup
-- Run this after database creation to ensure everything works
-- ============================================================

USE ManamDB;
GO

PRINT '========================================';
PRINT 'Manam Database - Connection Test';
PRINT '========================================';
PRINT '';

-- 1. Verify we're connected to the right database
PRINT 'Current Database:';
SELECT DB_NAME() AS [DatabaseName];
PRINT '';

-- 2. Test table access
PRINT 'Testing Table Access:';
BEGIN TRY
	SELECT COUNT(*) AS [UserCount] FROM [dbo].[Users];
	PRINT '✓ Users table accessible';
END TRY
BEGIN CATCH
	PRINT '✗ Error accessing Users table: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- 3. Test stored procedure access
PRINT 'Testing Stored Procedures:';

DECLARE @TestCount INT = 0;

-- Test sp_GetAllUsers
BEGIN TRY
	EXEC sp_GetAllUsers;
	SET @TestCount = @TestCount + 1;
	PRINT '✓ sp_GetAllUsers is accessible';
END TRY
BEGIN CATCH
	PRINT '✗ Error with sp_GetAllUsers: ' + ERROR_MESSAGE();
END CATCH

-- Test sp_CreateUser
BEGIN TRY
	DECLARE @TestId UNIQUEIDENTIFIER = NEWID();
	EXEC sp_CreateUser
		@Id = @TestId,
		@Username = 'ConnectionTest_' + CAST(GETDATE() AS NVARCHAR(30)),
		@Email = 'test_connection_' + CAST(GETDATE() AS NVARCHAR(30)) + '@test.com',
		@PasswordHash = 'test_hash',
		@FirstName = 'Connection',
		@LastName = 'Test',
		@IsActive = 1;

	SET @TestCount = @TestCount + 1;
	PRINT '✓ sp_CreateUser is working';

	-- Cleanup
	DELETE FROM [dbo].[Users] WHERE [Username] LIKE 'ConnectionTest_%';
END TRY
BEGIN CATCH
	PRINT '✗ Error with sp_CreateUser: ' + ERROR_MESSAGE();
END CATCH

PRINT '';
PRINT 'Stored Procedures Verified: ' + CAST(@TestCount AS NVARCHAR(2)) + '/2';
PRINT '';

-- 4. Database Statistics
PRINT 'Database Statistics:';
SELECT 
	DB_NAME() AS [DatabaseName],
	CONVERT(DECIMAL(10,2), (SUM(size) * 8.0 / 1024.0 / 1024.0)) AS [SizeMB]
FROM sys.master_files
WHERE database_id = DB_ID()
GROUP BY database_id;
PRINT '';

-- 5. Connection Strings
PRINT 'Connection Strings for appsettings.json:';
PRINT '';
PRINT '["Windows Authentication - Recommended"]';
PRINT 'Server=DESKTOP-O4ATQ75\MSSQLSERVER2;Database=ManamDB;Integrated Security=true;TrustServerCertificate=true;';
PRINT '';
PRINT '["Alternative - using localhost"]';
PRINT 'Server=localhost\MSSQLSERVER2;Database=ManamDB;Integrated Security=true;TrustServerCertificate=true;';
PRINT '';

-- 6. Configuration Check
PRINT 'Configuration Status:';
PRINT '';
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'ManamDB')
	PRINT '✓ Database ManamDB exists';
ELSE
	PRINT '✗ Database ManamDB not found';

IF OBJECT_ID('[dbo].[Users]') IS NOT NULL
	PRINT '✓ Users table exists';
ELSE
	PRINT '✗ Users table not found';

IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND name = 'sp_GetUserById')
	PRINT '✓ Stored procedures exist';
ELSE
	PRINT '✗ Stored procedures not found';

PRINT '';
PRINT '========================================';
PRINT 'Connection Test Complete';
PRINT '========================================';

GO
