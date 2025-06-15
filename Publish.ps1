#Requires -Version 5
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$scriptPath = $PSScriptRoot
$outputBaseDir = Join-Path $scriptPath "Publish"
$outputDir = Join-Path $outputBaseDir "bin"

Write-Host "Cleaning old publish directory..."
if (Test-Path $outputDir) {
    Remove-Item -Recurse -Force $outputDir
}
New-Item -ItemType Directory -Force $outputDir | Out-Null

$machineProject = Join-Path $scriptPath "SiliconeTrader.Machine/SiliconeTrader.Machine.csproj"
$uiProject = Join-Path $scriptPath "SiliconeTrader.UI/SiliconeTrader.UI.csproj"

$machineOutputDir = Join-Path $outputDir "SiliconeTrader.Machine"
$uiOutputDir = Join-Path $outputDir "SiliconeTrader.UI"

Write-Host "Publishing SiliconeTrader.Machine..."
dotnet publish $machineProject -c Release -o $machineOutputDir --nologo
If (-Not $?) { Write-Error "Failed to publish SiliconeTrader.Machine"; exit 1 }

Write-Host "Publishing SiliconeTrader.UI..."
dotnet publish $uiProject -c Release -o $uiOutputDir --nologo
If (-Not $?) { Write-Error "Failed to publish SiliconeTrader.UI"; exit 1 }

Write-Host "All done!"
