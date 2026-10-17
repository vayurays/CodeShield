<#
.SYNOPSIS
    Builds the solution and obfuscates the target assembly using CodeShieldCli.
.DESCRIPTION
    Demonstrates build script integration for CI/CD pipelines (PowerShell, GitHub Actions, Azure DevOps).
#>

param(
    [string]$Configuration = "Release",
    [switch]$VerboseOutput
)

$ErrorActionPreference = "Stop"
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " CodeShield Automated Build & Obfuscation Pipeline" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

$rootDir = $PSScriptRoot
$sampleProj = Join-Path $rootDir "samples\CodeShieldSampleApp\CodeShieldSampleApp.csproj"
$cliProj = Join-Path $rootDir "src\CodeShieldCli\CodeShieldCli.csproj"

# Step 1: Build Sample App
Write-Host "`n[1/3] Building target project ($Configuration)..." -ForegroundColor Yellow
dotnet build $sampleProj -c $Configuration --nologo
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

$targetDll = Join-Path $rootDir "samples\CodeShieldSampleApp\bin\$Configuration\net10.0\CodeShieldSampleApp.dll"
$obfuscatedDll = Join-Path $rootDir "samples\CodeShieldSampleApp\bin\$Configuration\net10.0\CodeShieldSampleApp.Protected.dll"
$mapFile = Join-Path $rootDir "samples\CodeShieldSampleApp\bin\$Configuration\net10.0\CodeShieldSampleApp.Protected.map.json"

# Step 2: Obfuscate with CodeShield CLI
Write-Host "`n[2/3] Running CodeShield CLI Obfuscation..." -ForegroundColor Yellow
$cliArgs = @(
    "run", "--project", $cliProj, "--",
    "-i", $targetDll,
    "-o", $obfuscatedDll,
    "-m", $mapFile,
    "--scheme", "confusing",
    "--no-preserve-public",
    "--encrypt-strings"
)

if ($VerboseOutput) {
    $cliArgs += "-v"
}

dotnet @cliArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error "CodeShield CLI returned failure exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# Step 3: Run the Obfuscated Assembly to verify runtime behavior
Write-Host "`n[3/3] Verifying runtime execution of protected assembly..." -ForegroundColor Yellow
$testOutput = dotnet $obfuscatedDll
Write-Host $testOutput -ForegroundColor Green

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n[PIPELINE SUCCESS] Target successfully obfuscated, verified, and ready for deployment!" -ForegroundColor Green
    Write-Host "  Input:       $targetDll"
    Write-Host "  Protected:   $obfuscatedDll"
    Write-Host "  Symbol Map:  $mapFile"
} else {
    Write-Error "Obfuscated binary failed runtime execution with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}
