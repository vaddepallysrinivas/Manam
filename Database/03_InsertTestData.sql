-- ============================================================
-- Insert Test Data
-- ============================================================
-- Purpose: Insert sample data for testing
-- ============================================================

USE ManamDB;
GO

PRINT '========== Inserting Test Data ==========';
PRINT '';

-- Insert test users
DECLARE @User1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @User2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @User3Id UNIQUEIDENTIFIER = NEWID();

PRINT 'Creating test users...';

-- Test User 1
EXEC sp_CreateUser
	@Id = @User1Id,
	@Username = 'admin',
	@Email = 'admin@manam.com',
	@PasswordHash = '$2a$12$test.hash.for.admin.password123',
	@FirstName = 'Admin',
	@LastName = 'User',
	@IsActive = 1;

PRINT 'User 1 created: ' + CAST(@User1Id AS NVARCHAR(36));

-- Test User 2
EXEC sp_CreateUser
	@Id = @User2Id,
	@Username = 'john.doe',
	@Email = 'john.doe@example.com',
	@PasswordHash = '$2a$12$test.hash.for.john.doe.password123',
	@FirstName = 'John',
	@LastName = 'Doe',
	@IsActive = 1;

PRINT 'User 2 created: ' + CAST(@User2Id AS NVARCHAR(36));

-- Test User 3
EXEC sp_CreateUser
	@Id = @User3Id,
	@Username = 'jane.smith',
	@Email = 'jane.smith@example.com',
	@PasswordHash = '$2a$12$test.hash.for.jane.smith.password123',
	@FirstName = 'Jane',
	@LastName = 'Smith',
	@IsActive = 1;

PRINT 'User 3 created: ' + CAST(@User3Id AS NVARCHAR(36));

PRINT '';
PRINT '========== Verifying Test Data ==========';
PRINT '';

-- Retrieve all test users
PRINT 'All Users:';
EXEC sp_GetAllUsers;

PRINT '';

-- Test getting by username
PRINT 'Get user by username (admin):';
EXEC sp_GetUserByUsername @Username = 'admin';

PRINT '';

-- Test getting by email
PRINT 'Get user by email (john.doe@example.com):';
EXEC sp_GetUserByEmail @Email = 'john.doe@example.com';

PRINT '';
PRINT '========== Test Data Insertion Complete ==========';

GO
