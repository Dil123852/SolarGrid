# Script: create-test-users.ps1
# Purpose: Manually create test users with all three roles via API endpoints
# Usage: .\create-test-users.ps1 -ApiUrl http://localhost:5000 -AdminUsername admin -AdminPassword <password>

param(
    [string]$ApiUrl = "http://localhost:5000",
    [string]$AdminUsername = "admin",
    [string]$AdminPassword = ""
)

# Colors for output
$ErrorColor = "Red"
$SuccessColor = "Green"
$WarningColor = "Yellow"

function Write-Success {
    param([string]$Message)
    Write-Host $Message -ForegroundColor $SuccessColor
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host $Message -ForegroundColor $ErrorColor
}

function Write-Warning-Custom {
    param([string]$Message)
    Write-Host $Message -ForegroundColor $WarningColor
}

# Get JWT token for admin
Write-Host "Authenticating as Backoffice user '$AdminUsername'..."
$loginBody = @{
    username = $AdminUsername
    password = $AdminPassword
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$ApiUrl/api/auth/login" `
        -Method Post `
        -ContentType "application/json" `
        -Body $loginBody

    $token = $loginResponse.token
    Write-Success "✓ Authentication successful"
}
catch {
    Write-Error-Custom "✗ Failed to authenticate: $($_.Exception.Message)"
    exit 1
}

# Headers with token
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Staff users to create
$staffUsers = @(
    @{
        username = "backoffice1"
        email = "backoffice1@solargrid.lk"
        password = "Test@12345"
        role = "Backoffice"
    },
    @{
        username = "backoffice2"
        email = "backoffice2@solargrid.lk"
        password = "Test@12345"
        role = "Backoffice"
    },
    @{
        username = "gridoperator1"
        email = "gridoperator1@solargrid.lk"
        password = "Test@12345"
        role = "GridOperator"
    },
    @{
        username = "gridoperator2"
        email = "gridoperator2@solargrid.lk"
        password = "Test@12345"
        role = "GridOperator"
    }
)

# Create staff users
Write-Host "`nCreating staff users..."
foreach ($user in $staffUsers) {
    $body = $user | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$ApiUrl/api/auth/register" `
            -Method Post `
            -ContentType "application/json" `
            -Headers $headers `
            -Body $body

        Write-Success "✓ Created $($user.role): $($user.username)"
    }
    catch {
        $errorMsg = $_.Exception.Response.Content | ConvertFrom-Json
        Write-Warning-Custom "⚠ $($user.role) $($user.username): $($errorMsg.message)"
    }
}

# Prosumers to create (via public endpoint)
$prosumers = @(
    @{
        nic = "199801011234"
        name = "Prosumer One"
        email = "prosumer1@example.lk"
        phone = "+94701234567"
        address = "123 Solar Street, Colombo"
    },
    @{
        nic = "199802022345"
        name = "Prosumer Two"
        email = "prosumer2@example.lk"
        phone = "+94702234567"
        address = "456 Energy Ave, Kandy"
    },
    @{
        nic = "199803033456"
        name = "Prosumer Three"
        email = "prosumer3@example.lk"
        phone = "+94703234567"
        address = "789 Grid Lane, Galle"
    }
)

# Create prosumers (no auth needed for registration)
Write-Host "`nCreating prosumer users..."
foreach ($prosumer in $prosumers) {
    $body = $prosumer | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$ApiUrl/api/prosumers" `
            -Method Post `
            -ContentType "application/json" `
            -Body $body

        Write-Success "✓ Created Prosumer: $($prosumer.name) (NIC: $($prosumer.nic))"
    }
    catch {
        $errorMsg = $_.Exception.Response.Content | ConvertFrom-Json
        Write-Warning-Custom "⚠ Prosumer $($prosumer.name): $($errorMsg.message)"
    }
}

Write-Host "`n--- Test User Creation Summary ---"
Write-Host "Staff Users (Backoffice): 2"
Write-Host "Staff Users (GridOperator): 2"
Write-Host "Prosumer Users: 3"
Write-Host "`nAll test users have been created (or already existed)."
Write-Host "Login to the web portal with any Backoffice or GridOperator user."
Write-Host "Check TEST_USERS.md for complete credentials list."
