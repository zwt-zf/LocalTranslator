[CmdletBinding()]
param(
    [ValidateSet('x64', 'ARM64')]
    [string]$Platform = 'x64',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [switch]$FullDictionary
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$dictionaryPath = Join-Path $repoRoot 'Community.PowerToys.Run.Plugin.LocalTranslator\Data\localtranslator.dict'

if ($FullDictionary -or -not (Test-Path -LiteralPath $dictionaryPath)) {
    & (Join-Path $PSScriptRoot 'build-dictionary.ps1') -Full:$FullDictionary
}

$dotnet = Get-Command dotnet -ErrorAction Stop
& $dotnet.Source build (Join-Path $repoRoot 'LocalTranslator.slnx') `
    --configuration $Configuration `
    --property:Platform=$Platform `
    --nologo

if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE."
}

$outputDirectory = Join-Path $repoRoot "Community.PowerToys.Run.Plugin.LocalTranslator\bin\$Platform\$Configuration\net9.0-windows10.0.26100.0"
Write-Host "Plugin output: $outputDirectory"
