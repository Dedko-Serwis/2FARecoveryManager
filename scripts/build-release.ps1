param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root
[xml]$project = Get-Content (Join-Path $root "2FARecoveryManager.csproj")
$version = [string]$project.Project.PropertyGroup.Version
if ([string]::IsNullOrWhiteSpace($version)) { throw "Missing Version in 2FARecoveryManager.csproj" }
dotnet test "tests/2FARecoveryManager.Tests.csproj" -c $Configuration
if ($LASTEXITCODE -ne 0) { throw "Tests failed" }
$publish = Join-Path $root "dist/publish"
dotnet publish "2FARecoveryManager.csproj" -c $Configuration -r $Runtime --self-contained false -p:PublishSingleFile=false -o $publish
if ($LASTEXITCODE -ne 0) { throw "Publish failed" }
$iscc = @(
    "${env:ProgramFiles(x86)}\Inno Setup 7\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 7\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) { throw "Inno Setup 7 ISCC.exe not found" }
& $iscc "/DAppVersion=$version" "/DPublishDir=$publish" "/DOutputDir=$(Join-Path $root 'dist')" "installer/2FARecoveryManager.iss"
if ($LASTEXITCODE -ne 0) { throw "Installer build failed" }
Write-Host "Installer created in dist."
