-- ============================================================
-- Create Manam Database
-- ============================================================
-- Script: 01_CreateDatabase.sql
-- Purpose: Creates the ManamDB database
-- Target: SQL Server (DESKTOP-O4ATQ75\MSSQLSERVER2)
-- ============================================================

-- Check if database exists and drop it
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'ManamDB')
BEGIN
	-- Close any existing connections
	ALTER DATABASE ManamDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
	DROP DATABASE ManamDB;
END
GO

-- Create the database
CREATE DATABASE ManamDB;
GO

-- Use the database
USE ManamDB;
GO

-- ============================================================
-- Create Tables
-- ============================================================

-- Create Users table
CREATE TABLE [dbo].[Users]
(
	[Id]                        UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	[Username]                  NVARCHAR(256) NOT NULL UNIQUE,
	[Email]                     NVARCHAR(354) NOT NULL UNIQUE,
	[PasswordHash]              NVARCHAR(MAX) NOT NULL,
	[FirstName]                 NVARCHAR(100) NOT NULL,
	[LastName]                  NVARCHAR(100) NOT NULL,
	[IsActive]                  BIT NOT NULL DEFAULT 1,
	[FailedLoginAttempts]       INT NULL DEFAULT 0,
	[LockoutUntil]              DATETIME2 NULL,
	[CreatedAt]                 DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
	[UpdatedAt]                 DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- Create indexes on Users table
CREATE NONCLUSTERED INDEX [IX_Users_Username] ON [dbo].[Users]([Username]);
CREATE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]([Email]);
CREATE NONCLUSTERED INDEX [IX_Users_IsActive] ON [dbo].[Users]([IsActive]);
GO

-- ============================================================
-- Create Stored Procedures
-- ============================================================

-- sp_GetUserById
CREATE PROCEDURE [dbo].[sp_GetUserById]
	@Id UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		[Id],
		[Username],
		[Email],
		[PasswordHash],
		[FirstName],
		[LastName],
		[IsActive],
		[FailedLoginAttempts],
		[LockoutUntil],
		[CreatedAt],
		[UpdatedAt]
	FROM [dbo].[Users]
	WHERE [Id] = @Id;
END;
GO

-- sp_GetUserByUsername
CREATE PROCEDURE [dbo].[sp_GetUserByUsername]
	@Username NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		[Id],
		[Username],
		[Email],
		[PasswordHash],
		[FirstName],
		[LastName],
		[IsActive],
		[FailedLoginAttempts],
		[LockoutUntil],
		[CreatedAt],
		[UpdatedAt]
	FROM [dbo].[Users]
	WHERE [Username] = @Username;
END;
GO

-- sp_GetUserByEmail
CREATE PROCEDURE [dbo].[sp_GetUserByEmail]
	@Email NVARCHAR(354)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		[Id],
		[Username],
		[Email],
		[PasswordHash],
		[FirstName],
		[LastName],
		[IsActive],
		[FailedLoginAttempts],
		[LockoutUntil],
		[CreatedAt],
		[UpdatedAt]
	FROM [dbo].[Users]
	WHERE [Email] = @Email;
END;
GO

-- sp_GetAllUsers
CREATE PROCEDURE [dbo].[sp_GetAllUsers]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		[Id],
		[Username],
		[Email],
		[PasswordHash],
		[FirstName],
		[LastName],
		[IsActive],
		[FailedLoginAttempts],
		[LockoutUntil],
		[CreatedAt],
		[UpdatedAt]
	FROM [dbo].[Users]
	ORDER BY [CreatedAt] DESC;
END;
GO

-- sp_CreateUser
CREATE PROCEDURE [dbo].[sp_CreateUser]
	@Id UNIQUEIDENTIFIER,
	@Username NVARCHAR(256),
	@Email NVARCHAR(354),
	@PasswordHash NVARCHAR(MAX),
	@FirstName NVARCHAR(100),
	@LastName NVARCHAR(100),
	@IsActive BIT = 1
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		INSERT INTO [dbo].[Users]
		(
			[Id],
			[Username],
			[Email],
			[PasswordHash],
			[FirstName],
			[LastName],
			[IsActive],
			[FailedLoginAttempts],
			[CreatedAt],
			[UpdatedAt]
		)
		VALUES
		(
			@Id,
			@Username,
			@Email,
			@PasswordHash,
			@FirstName,
			@LastName,
			@IsActive,
			0,
			GETUTCDATE(),
			GETUTCDATE()
		);

		SELECT @Id AS [Id];
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

-- sp_UpdateUser
CREATE PROCEDURE [dbo].[sp_UpdateUser]
	@Id UNIQUEIDENTIFIER,
	@Username NVARCHAR(256),
	@Email NVARCHAR(354),
	@PasswordHash NVARCHAR(MAX),
	@FirstName NVARCHAR(100),
	@LastName NVARCHAR(100),
	@IsActive BIT
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		UPDATE [dbo].[Users]
		SET
			[Username] = @Username,
			[Email] = @Email,
			[PasswordHash] = @PasswordHash,
			[FirstName] = @FirstName,
			[LastName] = @LastName,
			[IsActive] = @IsActive,
			[UpdatedAt] = GETUTCDATE()
		WHERE [Id] = @Id;

		SELECT @@ROWCOUNT AS [RowsAffected];
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

-- sp_DeleteUser
CREATE PROCEDURE [dbo].[sp_DeleteUser]
	@Id UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		DELETE FROM [dbo].[Users]
		WHERE [Id] = @Id;

		SELECT @@ROWCOUNT AS [RowsAffected];
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

-- sp_UpdateUserLockout
CREATE PROCEDURE [dbo].[sp_UpdateUserLockout]
	@Id UNIQUEIDENTIFIER,
	@LockoutUntil DATETIME2
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		UPDATE [dbo].[Users]
		SET
			[LockoutUntil] = @LockoutUntil,
			[UpdatedAt] = GETUTCDATE()
		WHERE [Id] = @Id;

		SELECT @@ROWCOUNT AS [RowsAffected];
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

-- sp_ResetFailedLoginAttempts
CREATE PROCEDURE [dbo].[sp_ResetFailedLoginAttempts]
	@Id UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		UPDATE [dbo].[Users]
		SET
			[FailedLoginAttempts] = 0,
			[LockoutUntil] = NULL,
			[UpdatedAt] = GETUTCDATE()
		WHERE [Id] = @Id;

		SELECT @@ROWCOUNT AS [RowsAffected];
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

-- ============================================================
-- Print Summary
-- ============================================================
PRINT '========================================';
PRINT 'Manam Database Setup Complete!';
PRINT '========================================';
PRINT 'Database: ManamDB';
PRINT 'Tables created:';
PRINT '  - Users';
PRINT '';
PRINT 'Stored Procedures created:';
PRINT '  - sp_GetUserById';
PRINT '  - sp_GetUserByUsername';
PRINT '  - sp_GetUserByEmail';
PRINT '  - sp_GetAllUsers';
PRINT '  - sp_CreateUser';
PRINT '  - sp_UpdateUser';
PRINT '  - sp_DeleteUser';
PRINT '  - sp_UpdateUserLockout';
PRINT '  - sp_ResetFailedLoginAttempts';
PRINT '';
PRINT 'Connection String:';
PRINT 'Server=DESKTOP-O4ATQ75\MSSQLSERVER2;Database=ManamDB;Integrated Security=true;TrustServerCertificate=true;';
PRINT '========================================';
GO
