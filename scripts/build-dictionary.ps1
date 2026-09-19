[CmdletBinding()]
param(
    [switch]$Full,
    [switch]$SkipDownload
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$builderProject = Join-Path $repoRoot 'tools\LocalTranslator.DictionaryBuilder\LocalTranslator.DictionaryBuilder.csproj'
$outputPath = Join-Path $repoRoot 'Community.PowerToys.Run.Plugin.LocalTranslator\Data\localtranslator.dict'

if ($Full) {
    $sourceDirectory = Join-Path $repoRoot 'artifacts\dictionary-sources'
    New-Item -ItemType Directory -Path $sourceDirectory -Force | Out-Null
    $ecdictPath = Join-Path $sourceDirectory 'ecdict.csv'
    $cedictArchive = Join-Path $sourceDirectory 'cedict.zip'
    $cedictDirectory = Join-Path $sourceDirectory 'cedict'

    if (-not $SkipDownload) {
        Write-Host 'Downloading ECDICT source data...'
        Invoke-WebRequest -UseBasicParsing -Uri 'https://raw.githubusercontent.com/skywind3000/ECDICT/master/ecdict.csv' -OutFile $ecdictPath

        Write-Host 'Downloading CC-CEDICT source data...'
        Invoke-WebRequest -UseBasicParsing -Uri 'https://cc-cedict.org/editor/editor_export_cedict.php?c=zip' -OutFile $cedictArchive
        New-Item -ItemType Directory -Path $cedictDirectory -Force | Out-Null
        Expand-Archive -LiteralPath $cedictArchive -DestinationPath $cedictDirectory -Force
    }

    $cedictPath = Get-ChildItem -LiteralPath $cedictDirectory -File |
        Where-Object { ($_.Extension -in @('.u8', '.txt')) -and $_.Name -match 'cedict' } |
        Select-Object -First 1 -ExpandProperty FullName

    if (-not (Test-Path -LiteralPath $ecdictPath) -or -not $cedictPath) {
        throw 'Full dictionary source files are missing. Run without -SkipDownload first.'
    }
}
else {
    $ecdictPath = Join-Path $repoRoot 'dictionary-sources\seed\ecdict.csv'
    $cedictPath = Join-Path $repoRoot 'dictionary-sources\seed\cedict_ts.u8'
}

$dotnet = Get-Command dotnet -ErrorAction Stop
& $dotnet.Source run --project $builderProject --configuration Release -- `
    --output $outputPath `
    --ecdict $ecdictPath `
    --cedict $cedictPath

if ($LASTEXITCODE -ne 0) {
    throw "Dictionary build failed with exit code $LASTEXITCODE."
}
