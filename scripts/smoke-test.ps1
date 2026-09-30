<#
 File: smoke-test.ps1
 Purpose: End-to-end check of the running SolarGrid API against the real database - role locks
          and every business rule. Creates clearly labelled SMOKE test records and deactivates
          them at the end.
 Usage:   (start the API first:  dotnet run --project api/SolarGrid.Api --launch-profile http)
          powershell -ExecutionPolicy Bypass -File scripts\smoke-test.ps1
          powershell -ExecutionPolicy Bypass -File scripts\smoke-test.ps1 -BaseUrl http://localhost:8080
 Project: Smart Solar Microgrid Trading System - SE4040 Assignment 1
#>
param(
    [string]$BaseUrl = "http://localhost:5093",
    [string]$AdminUser = "admin",
    [string]$AdminPassword
)

$ErrorActionPreference = "Stop"
$script:passed = 0
$script:failed = 0

# Admin password defaults to the seed password in the git-ignored local settings file.
if (-not $AdminPassword) {
    $local = Join-Path $PSScriptRoot "..\api\SolarGrid.Api\appsettings.Local.json"
    $AdminPassword = (Get-Content $local -Raw | ConvertFrom-Json).Seed.BackofficePassword
}

# Calls the API and returns { Status, Body } without throwing on 4xx/5xx (works on PS 5.1 and 7).
function Invoke-Api([string]$Method, [string]$Path, $Body = $null, [string]$Token = $null) {
    $headers = @{}
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }
    $params = @{ Uri = "$BaseUrl$Path"; Method = $Method; Headers = $headers; UseBasicParsing = $true; ContentType = "application/json" }
    if ($null -ne $Body) { $params["Body"] = ($Body | ConvertTo-Json -Depth 5) }
    try {
        $r = Invoke-WebRequest @params
        $json = $null
        if ($r.Content) { $json = $r.Content | ConvertFrom-Json }
        return [pscustomobject]@{ Status = [int]$r.StatusCode; Body = $json }
    } catch {
        $resp = $_.Exception.Response
        if ($null -eq $resp) { throw "Cannot reach $BaseUrl - is the API running? ($($_.Exception.Message))" }
        $text = $_.ErrorDetails.Message
        if (-not $text) {
            try { $text = (New-Object System.IO.StreamReader($resp.GetResponseStream())).ReadToEnd() } catch { $text = "" }
        }
        $json = $null
        if ($text) { try { $json = $text | ConvertFrom-Json } catch { } }
        return [pscustomobject]@{ Status = [int]$resp.StatusCode; Body = $json }
    }
}

function Expect([string]$Name, $Result, [int]$Expected) {
    $msg = ""
    if ($Result.Body -and $Result.Body.PSObject.Properties["message"]) { $msg = " - " + $Result.Body.message }
    if ($Result.Status -eq $Expected) {
        $script:passed++
        Write-Host ("  PASS  {0}  [{1}]{2}" -f $Name, $Result.Status, $msg) -ForegroundColor Green
    } else {
        $script:failed++
        Write-Host ("  FAIL  {0}  expected {1}, got {2}{3}" -f $Name, $Expected, $Result.Status, $msg) -ForegroundColor Red
    }
    return $Result
}

function Slot([double]$Days) {
    # Whole-hour UTC slot, sent as ISO 8601 with Z.
    $t = (Get-Date).ToUniversalTime().AddDays($Days)
    return (Get-Date -Year $t.Year -Month $t.Month -Day $t.Day -Hour $t.Hour -Minute 0 -Second 0).ToString("yyyy-MM-ddTHH:00:00Z")
}

$suffix = Get-Random -Minimum 1000 -Maximum 9999
$nic = "2000" + (Get-Random -Minimum 10000000 -Maximum 99999999)
Write-Host "SolarGrid smoke test against $BaseUrl (run $suffix)`n" -ForegroundColor Cyan

Write-Host "Authentication & role locks"
Expect "Nodes without a token -> 401" (Invoke-Api GET "/api/nodes") 401 | Out-Null
$admin = Expect "Backoffice login" (Invoke-Api POST "/api/auth/login" @{ username = $AdminUser; password = $AdminPassword }) 200
if ($admin.Status -ne 200) { Write-Host "`nCannot continue without the admin login. Check Seed:BackofficePassword." -ForegroundColor Red; exit 1 }
$adminToken = $admin.Body.token
Expect "Users list as Backoffice -> 200" (Invoke-Api GET "/api/users" $null $adminToken) 200 | Out-Null

$opName = "smoke-op-$suffix"
Expect "Create Grid Operator" (Invoke-Api POST "/api/auth/register" @{ username = $opName; email = "$opName@solargrid.lk"; password = "Operator1!"; role = "GridOperator" } $adminToken) 200 | Out-Null
$opLogin = Expect "Grid Operator login" (Invoke-Api POST "/api/auth/login" @{ username = $opName; password = "Operator1!" }) 200
$opToken = $opLogin.Body.token
Expect "Users list as Grid Operator -> 403" (Invoke-Api GET "/api/users" $null $opToken) 403 | Out-Null

Write-Host "`nSetup: node and prosumer"
$node = Expect "Create node" (Invoke-Api POST "/api/nodes" @{ name = "SMOKE Node $suffix"; latitude = 6.9271; longitude = 79.8612; capacityKWh = 50; batterySlots = 5 } $adminToken) 200
$nodeId = $node.Body.id
Expect "Register prosumer (NIC $nic)" (Invoke-Api POST "/api/prosumers" @{ nic = $nic; name = "Smoke Tester"; email = "smoke$suffix@example.com"; phone = "0771234567"; address = "Colombo"; password = "Prosumer1!" }) 200 | Out-Null
$pLogin = Expect "Prosumer login" (Invoke-Api POST "/api/auth/prosumer-login" @{ nic = $nic; password = "Prosumer1!" }) 200
$pToken = $pLogin.Body.token

Write-Host "`nReservation rules"
Expect "Book 8 days out -> 400" (Invoke-Api POST "/api/reservations" @{ nodeId = $nodeId; slotTime = (Slot 8) } $pToken) 400 | Out-Null
$r1 = Expect "Book 2 days out" (Invoke-Api POST "/api/reservations" @{ nodeId = $nodeId; slotTime = (Slot 2) } $pToken) 200
Expect "Reschedule to 3 days out" (Invoke-Api PUT "/api/reservations/$($r1.Body.id)" @{ slotTime = (Slot 3) } $pToken) 200 | Out-Null
Expect "Cancel it" (Invoke-Api DELETE "/api/reservations/$($r1.Body.id)" $null $pToken) 200 | Out-Null
Expect "Approve cancelled booking -> 400" (Invoke-Api POST "/api/reservations/$($r1.Body.id)/approve" $null $adminToken) 400 | Out-Null

Write-Host "`nQR verification"
$r2 = Expect "Book 4 days out" (Invoke-Api POST "/api/reservations" @{ nodeId = $nodeId; slotTime = (Slot 4) } $pToken) 200
$approved = Expect "Approve -> QR token" (Invoke-Api POST "/api/reservations/$($r2.Body.id)/approve" $null $adminToken) 200
$qr = $approved.Body.qrToken
Expect "Prosumer cannot verify QR -> 403" (Invoke-Api POST "/api/reservations/verify-qr" @{ qrToken = $qr } $pToken) 403 | Out-Null
Expect "Operator scans QR -> 200" (Invoke-Api POST "/api/reservations/verify-qr" @{ qrToken = $qr } $opToken) 200 | Out-Null
Expect "Same QR again -> 400" (Invoke-Api POST "/api/reservations/verify-qr" @{ qrToken = $qr } $opToken) 400 | Out-Null

Write-Host "`nNode deactivation rule"
$r3 = Expect "Book 5 days out (pending)" (Invoke-Api POST "/api/reservations" @{ nodeId = $nodeId; slotTime = (Slot 5) } $pToken) 200
Expect "Deactivate node with pending booking -> 409" (Invoke-Api PUT "/api/nodes/$nodeId/deactivate" $null $adminToken) 409 | Out-Null
$dash = Expect "Prosumer dashboard counts" (Invoke-Api GET "/api/dashboard" $null $pToken) 200
if ($dash.Status -eq 200) { Write-Host ("        pending={0} approvedFuture={1} completed={2}" -f $dash.Body.pendingCount, $dash.Body.approvedFutureCount, $dash.Body.completedCount) }

Write-Host "`nAccount deactivation / reactivation"
Expect "Prosumer deactivates self" (Invoke-Api PUT "/api/prosumers/$nic/deactivate" $null $pToken) 200 | Out-Null
Expect "Deactivated login -> 403 (pending activation)" (Invoke-Api POST "/api/auth/prosumer-login" @{ nic = $nic; password = "Prosumer1!" }) 403 | Out-Null
Expect "Backoffice reactivates" (Invoke-Api PUT "/api/prosumers/$nic/reactivate" $null $adminToken) 200 | Out-Null
$pLogin2 = Expect "Login works again" (Invoke-Api POST "/api/auth/prosumer-login" @{ nic = $nic; password = "Prosumer1!" }) 200

Write-Host "`nCleanup (deactivating SMOKE records)"
Expect "Cancel pending booking" (Invoke-Api DELETE "/api/reservations/$($r3.Body.id)" $null $pLogin2.Body.token) 200 | Out-Null
Expect "Deactivate node now allowed" (Invoke-Api PUT "/api/nodes/$nodeId/deactivate" $null $adminToken) 200 | Out-Null
$users = Invoke-Api GET "/api/users" $null $adminToken
$opUser = $users.Body | Where-Object { $_.username -eq $opName } | Select-Object -First 1
if ($opUser) {
    Expect "Disable test operator" (Invoke-Api PUT "/api/users/$($opUser.id)/deactivate" $null $adminToken) 200 | Out-Null
}

Write-Host ""
$color = "Green"
if ($script:failed -gt 0) { $color = "Red" }
Write-Host ("Result: {0} passed, {1} failed" -f $script:passed, $script:failed) -ForegroundColor $color
if ($script:failed -gt 0) { exit 1 }
