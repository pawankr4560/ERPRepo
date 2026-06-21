[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$PublishPath,

    [string]$SiteName = "ERPWebApp",
    [string]$AppPoolName = "ERPWebApp",
    [string]$PhysicalPath = "C:\inetpub\ERPWebApp",
    [int]$Port = 80,
    [string]$HostName = "",

    [Parameter(Mandatory)]
    [string]$ConnectionString,

    [Parameter(Mandatory)]
    [ValidateLength(32, 512)]
    [string]$JwtKey,

    [Parameter(Mandatory)]
    [string]$JwtIssuer,

    [Parameter(Mandatory)]
    [string]$JwtAudience,

    [string]$ApiKey = ""
)

$ErrorActionPreference = "Stop"

if (-not ([Security.Principal.WindowsPrincipal](
    [Security.Principal.WindowsIdentity]::GetCurrent()
)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Run this script from an elevated PowerShell window."
}

Import-Module WebAdministration

$resolvedPublishPath = (Resolve-Path -LiteralPath $PublishPath).Path
if (-not (Test-Path -LiteralPath (Join-Path $resolvedPublishPath "ERPWebApp.Server.dll"))) {
    throw "The publish folder does not contain ERPWebApp.Server.dll."
}

if (-not (Test-Path -LiteralPath $PhysicalPath)) {
    New-Item -ItemType Directory -Path $PhysicalPath -Force | Out-Null
}

if (Test-Path "IIS:\Sites\$SiteName") {
    Stop-Website -Name $SiteName
}

Get-ChildItem -LiteralPath $PhysicalPath -Force |
    Remove-Item -Recurse -Force
Copy-Item -Path (Join-Path $resolvedPublishPath "*") -Destination $PhysicalPath -Recurse -Force

$productionSettings = @{
    ConnectionStrings = @{
        Default = $ConnectionString
    }
    Jwt = @{
        Key = $JwtKey
        Issuer = $JwtIssuer
        Audience = $JwtAudience
    }
    api_key = $ApiKey
} | ConvertTo-Json -Depth 5

$productionSettings |
    Set-Content -LiteralPath (Join-Path $PhysicalPath "appsettings.Production.json") -Encoding UTF8

if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) {
    New-WebAppPool -Name $AppPoolName | Out-Null
}

Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedPipelineMode -Value "Integrated"

if (-not (Test-Path "IIS:\Sites\$SiteName")) {
    New-Website `
        -Name $SiteName `
        -ApplicationPool $AppPoolName `
        -PhysicalPath $PhysicalPath `
        -Port $Port `
        -HostHeader $HostName | Out-Null
} else {
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name physicalPath -Value $PhysicalPath
}

$acl = Get-Acl -LiteralPath $PhysicalPath
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    "IIS AppPool\$AppPoolName",
    "ReadAndExecute",
    "ContainerInherit,ObjectInherit",
    "None",
    "Allow")
$acl.SetAccessRule($accessRule)
Set-Acl -LiteralPath $PhysicalPath -AclObject $acl

Start-Website -Name $SiteName
Restart-WebAppPool -Name $AppPoolName

Write-Host "IIS deployment completed."
Write-Host "Site: $SiteName"
Write-Host "Path: $PhysicalPath"
Write-Host "Test URL: http://localhost:$Port/test"
