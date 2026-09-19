[CmdletBinding()]
param(
    [ValidateSet('x64', 'ARM64')]
    [string]$Platform = 'x64',
    [switch]$FullDictionary
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
& (Join-Path $PSScriptRoot 'build.ps1') -Platform $Platform -Configuration Release -FullDictionary:$FullDictionary

$outputDirectory = Join-Path $repoRoot "Community.PowerToys.Run.Plugin.LocalTranslator\bin\$Platform\Release\net9.0-windows10.0.26100.0"
$artifactDirectory = Join-Path $repoRoot 'artifacts'
$packagePath = Join-Path $artifactDirectory "LocalTranslator-0.1.1-$Platform.zip"
New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null

Compress-Archive -Path (Join-Path $outputDirectory '*') -DestinationPath $packagePath -Force
Write-Host "Package: $packagePath"
