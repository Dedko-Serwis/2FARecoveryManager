$ErrorActionPreference = "Stop"

$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$project = Join-Path $root "2FARecoveryManager.csproj"
$tests = Join-Path $root "tests\2FARecoveryManager.Tests.csproj"
$publish = Join-Path $root "dist\publish"
$installer = Join-Path $root "installer\2FARecoveryManager.iss"
$outputDir = Join-Path $root "dist"

# The .csproj file is the single source of the application version.
[xml]$projectXml = Get-Content -LiteralPath $project -Raw -Encoding UTF8
$Version = [string]$projectXml.Project.PropertyGroup.Version
if ($Version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+$') {
    throw "Invalid version in 2FARecoveryManager.csproj: '$Version' (expected X.Y.Z)."
}

Write-Host "Building 2FARecoveryManager $Version"
& dotnet test $tests -c Release
if ($LASTEXITCODE -ne 0) { throw "Tests failed." }

& dotnet publish $project -c Release -r win-x64 --self-contained false "-p:Version=$Version" "-p:AssemblyVersion=$Version.0" "-p:FileVersion=$Version.0" "-p:InformationalVersion=$Version" -o $publish
if ($LASTEXITCODE -ne 0) { throw "Publish failed." }

# Match the established MeterHUD installer lookup, including per-user installs.
$isccCandidates = @(
    "$env:LOCALAPPDATA\Programs\Inno Setup 7\ISCC.exe"
    "$env:ProgramFiles\Inno Setup 7\ISCC.exe"
    "${env:ProgramFiles(x86)}\Inno Setup 7\ISCC.exe"
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
)

foreach ($regPath in @(
    'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 7_is1',
    'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 7_is1',
    'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 7_is1',
    'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1',
    'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1',
    'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1'
)) {
    $install = Get-ItemProperty -Path $regPath -ErrorAction SilentlyContinue
    if ($install -and $install.InstallLocation) {
        $isccCandidates += Join-Path $install.InstallLocation 'ISCC.exe'
    }
}

$iscc = $isccCandidates |
    Where-Object { $_ -and (Test-Path -LiteralPath $_ -PathType Leaf) } |
    Select-Object -First 1

if (-not $iscc) {
    $cmd = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($cmd) { $iscc = $cmd.Source }
}
if (-not $iscc) { throw "Inno Setup 6 or 7 (ISCC.exe) was not found." }

Write-Host "Inno Setup: $iscc"
& $iscc "/DAppVersion=$Version" "/DPublishDir=$publish" "/DOutputDir=$outputDir" $installer
if ($LASTEXITCODE -ne 0) { throw "Installer build failed." }

$setup = Join-Path $outputDir "2FARecoveryManager-Setup-$Version.exe"
if (-not (Test-Path -LiteralPath $setup -PathType Leaf)) {
    throw "Installer not found: $setup"
}
Get-FileHash -LiteralPath $setup -Algorithm SHA256
