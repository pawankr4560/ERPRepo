$ErrorActionPreference = "Stop"
$logPath = Join-Path $PSScriptRoot "Enable-IIS-Prerequisites.log"
Start-Transcript -Path $logPath -Force

try {

if (-not ([Security.Principal.WindowsPrincipal](
    [Security.Principal.WindowsIdentity]::GetCurrent()
)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "This script must run as Administrator."
}

$features = @(
    "IIS-WebServerRole",
    "IIS-WebServer",
    "IIS-CommonHttpFeatures",
    "IIS-StaticContent",
    "IIS-DefaultDocument",
    "IIS-HttpErrors",
    "IIS-ApplicationDevelopment",
    "IIS-ISAPIExtensions",
    "IIS-ISAPIFilter",
    "IIS-ManagementConsole"
)

foreach ($feature in $features) {
    Enable-WindowsOptionalFeature `
        -Online `
        -FeatureName $feature `
        -All `
        -NoRestart `
        -ErrorAction Stop | Out-Null
}

if (-not (Test-Path "$env:ProgramFiles\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll")) {
    winget install `
        --id Microsoft.DotNet.HostingBundle.8 `
        --exact `
        --silent `
        --accept-package-agreements `
        --accept-source-agreements

    if ($LASTEXITCODE -notin 0, -1978335189) {
        throw "The .NET 8 Hosting Bundle installation failed with exit code $LASTEXITCODE."
    }
}

    & "$env:windir\System32\iisreset.exe"
}
catch {
    Write-Error $_
    exit 1
}
finally {
    Stop-Transcript
}
