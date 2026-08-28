# 打包 IslandCaller.TopmostEnhancer 插件为 .cipx
# 用法：powershell -File scripts\Package-Release.ps1 -Version 1.0.0.0 [-OutputDirectory <dir>]
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$')]
    [string]$Version,

    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repositoryRoot 'artifacts\release'
}

$releaseDirectory = Join-Path $OutputDirectory $Version
$pluginDirectory = Join-Path $releaseDirectory 'plugin'
$cipxPath = Join-Path $releaseDirectory 'IslandCaller.TopmostEnhancer.cipx'

if (Test-Path -LiteralPath $releaseDirectory) {
    throw "Output directory already exists: $releaseDirectory. Choose a different -OutputDirectory or remove it first."
}

New-Item -ItemType Directory -Path $pluginDirectory -Force | Out-Null

Push-Location $repositoryRoot
try {
    dotnet build 'IslandCaller.TopmostEnhancer.csproj' --configuration Release --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Failed to build IslandCaller.TopmostEnhancer.' }

    $pluginBuildDirectory = Join-Path $repositoryRoot 'bin\Release\net10.0'
    if (-not (Test-Path -LiteralPath $pluginBuildDirectory -PathType Container)) {
        throw "Plugin build output not found: $pluginBuildDirectory"
    }

    $pluginBuildFiles = @(Get-ChildItem -LiteralPath $pluginBuildDirectory -File |
        Where-Object {
            $_.Extension -ne '.pdb' -and
            $_.Name -ne 'IslandCaller.TopmostEnhancer.deps.json'
        })
    if ($pluginBuildFiles.Count -eq 0) {
        throw "Plugin build output is empty: $pluginBuildDirectory"
    }
    Copy-Item -LiteralPath $pluginBuildFiles.FullName -Destination $pluginDirectory
}
finally {
    Pop-Location
}

$manifestPath = Join-Path $pluginDirectory 'manifest.yml'
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "Built plugin manifest not found: $manifestPath"
}
$manifest = Get-Content -LiteralPath $manifestPath -Raw
if ($manifest -notmatch '(?m)^version: .*$') {
    throw "No version field was found in $manifestPath"
}
[System.IO.File]::WriteAllText(
    $manifestPath,
    [regex]::Replace($manifest, '(?m)^version: .*$', "version: $Version"),
    [System.Text.UTF8Encoding]::new($false))

$pluginContents = @(Get-ChildItem -LiteralPath $pluginDirectory -Force)
if ($pluginContents.Count -eq 0) {
    throw 'Plugin package output is empty.'
}

$zipPath = Join-Path $releaseDirectory 'IslandCaller.TopmostEnhancer.zip'
Compress-Archive -Path $pluginContents.FullName -DestinationPath $zipPath -CompressionLevel Optimal
Move-Item -LiteralPath $zipPath -Destination $cipxPath

$cipxMd5 = (Get-FileHash -LiteralPath $cipxPath -Algorithm MD5).Hash.ToLowerInvariant()
Write-Host "Plugin package: $cipxPath"
Write-Host "Plugin MD5: $cipxMd5"
Write-Host "Plugin size: $((Get-Item -LiteralPath $cipxPath).Length) bytes"
