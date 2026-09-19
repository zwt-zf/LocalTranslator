[CmdletBinding()]
param(
    [ValidateSet('x64', 'ARM64')]
    [string]$Platform = 'x64'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$sourceDirectory = Join-Path $repoRoot "Community.PowerToys.Run.Plugin.LocalTranslator\bin\$Platform\Release\net9.0-windows10.0.26100.0"
$pluginRoot = Join-Path $env:LOCALAPPDATA 'Microsoft\PowerToys\PowerToys Run\Plugins'
$destinationDirectory = Join-Path $pluginRoot 'LocalTranslator'

if (-not (Test-Path -LiteralPath $sourceDirectory)) {
    throw 'Release output is missing. Run scripts\build.ps1 first.'
}

if (Get-Process -Name PowerToys -ErrorAction SilentlyContinue) {
    throw 'Please exit PowerToys before installing the plugin, then run this script again.'
}

New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null
Copy-Item -Path (Join-Path $sourceDirectory '*') -Destination $destinationDirectory -Recurse -Force
Write-Host "Installed to: $destinationDirectory"
Write-Host 'Start PowerToys, open PowerToys Run, and type: tr hello'
