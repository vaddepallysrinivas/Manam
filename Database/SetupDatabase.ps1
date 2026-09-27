# ============================================================
# Manam Database Setup - PowerShell Helper Script
# ============================================================
# Purpose: Easily execute database scripts from PowerShell
# Usage: .\SetupDatabase.ps1
# ============================================================

param(
	[Parameter(Mandatory=$false)]
	[ValidateSet("Create", "Verify", "InsertTestData", "All")]
	[string]$Action = "Create",

	[Parameter(Mandatory=$false)]
	[string]$ServerName = "DESKTOP-O4ATQ75\MSSQLSERVER2",

	[Parameter(Mandatory=$false)]
	[string]$DatabaseName = "ManamDB"
)

# ============================================================
# Helper Functions
# ============================================================

function Write-Header {
	param([string]$Message)
	Write-Host ""
	Write-Host "========================================" -ForegroundColor Cyan
	Write-Host $Message -ForegroundColor Cyan
	Write-Host "========================================" -ForegroundColor Cyan
	Write-Host ""
}

function Write-Success {
	param([string]$Message)
	Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Error-Message {
	param([string]$Message)
	Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Info {
	param([string]$Message)
	Write-Host "ℹ️  $Message" -ForegroundColor Yellow
}

function Execute-SqlScript {
	param(
		[string]$ScriptPath,
		[string]$ServerName,
		[string]$DatabaseName
	)

	if (-not (Test-Path $ScriptPath)) {
		Write-Error-Message "Script not found: $ScriptPath"
		return $false
	}

	Write-Info "Executing: $ScriptPath"
	Write-Info "Server: $ServerName"
	Write-Info "Database: $DatabaseName"
	Write-Host ""

	try {
		if ($DatabaseName -eq "master") {
			# For scripts that create databases, connect to master
			sqlcmd -S $ServerName -i $ScriptPath -w 200
		} else {
			sqlcmd -S $ServerName -d $DatabaseName -i $ScriptPath -w 200
		}

		if ($LASTEXITCODE -eq 0) {
			Write-Success "Script executed successfully!"
			return $true
		} else {
			Write-Error-Message "Script execution failed with exit code: $LASTEXITCODE"
			return $false
		}
	}
	catch {
		Write-Error-Message "Error executing script: $_"
		return $false
	}
}

# ============================================================
# Main Script
# ============================================================

Write-Header "Manam Database Setup Tool"

Write-Info "Server: $ServerName"
Write-Info "Database: $DatabaseName"
Write-Info "Action: $Action"

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$DatabaseDir = Join-Path $ScriptDir

# Verify sqlcmd is available
Write-Info "Checking for sqlcmd..."
try {
	$sqlcmdVersion = sqlcmd -? 2>&1 | Select-Object -First 1
	Write-Success "sqlcmd found"
}
catch {
	Write-Error-Message "sqlcmd not found. Please install SQL Server Management Studio or SQL Server tools."
	Write-Info "Download: https://docs.microsoft.com/en-us/sql/tools/sqlcmd-utility"
	exit 1
}

# ============================================================
# Execute Actions
# ============================================================

$allSuccessful = $true

if ($Action -eq "Create" -or $Action -eq "All") {
	Write-Header "Step 1: Creating Database and Objects"

	$scriptPath = Join-Path $DatabaseDir "01_CreateDatabase.sql"
	$result = Execute-SqlScript -ScriptPath $scriptPath -ServerName $ServerName -DatabaseName "master"

	if ($result) {
		Write-Success "Database creation completed"
	} else {
		Write-Error-Message "Database creation failed"
		$allSuccessful = $false
	}

	Start-Sleep -Seconds 1
}

if ($Action -eq "Verify" -or $Action -eq "All") {
	Write-Header "Step 2: Verifying Database Objects"

	$scriptPath = Join-Path $DatabaseDir "02_VerifyDatabase.sql"
	$result = Execute-SqlScript -ScriptPath $scriptPath -ServerName $ServerName -DatabaseName $DatabaseName

	if ($result) {
		Write-Success "Database verification completed"
	} else {
		Write-Error-Message "Database verification failed"
		$allSuccessful = $false
	}
}

if ($Action -eq "InsertTestData" -or $Action -eq "All") {
	Write-Header "Step 3: Inserting Test Data"

	$scriptPath = Join-Path $DatabaseDir "03_InsertTestData.sql"
	$result = Execute-SqlScript -ScriptPath $scriptPath -ServerName $ServerName -DatabaseName $DatabaseName

	if ($result) {
		Write-Success "Test data insertion completed"
	} else {
		Write-Error-Message "Test data insertion failed"
		$allSuccessful = $false
	}
}

# ============================================================
# Summary
# ============================================================

Write-Header "Setup Summary"

if ($allSuccessful) {
	Write-Success "All operations completed successfully!"
	Write-Info "Database: $DatabaseName is ready to use"
	Write-Info "Connection string: Server=$ServerName;Database=$DatabaseName;Integrated Security=true;TrustServerCertificate=true;"
	Write-Host ""
	Write-Info "Next steps:"
	Write-Host "  1. Update appsettings.json if connection string differs"
	Write-Host "  2. Run your Manam API application"
	Write-Host "  3. Test endpoints with sample data"
}
else {
	Write-Error-Message "Some operations failed. Please check the output above."
	exit 1
}

Write-Host ""
