<#
.SYNOPSIS
    Builds ShalazamPlugin and packages it with the installer into a release zip.

.DESCRIPTION
    The build references the installed game's Il2Cpp assemblies, so releases can only be
    cut locally - there is no CI equivalent of this script. The version comes from
    ModMain.PluginVersion.

.EXAMPLE
    .\tools\package.ps1
    .\tools\package.ps1 -Configuration Debug -OutputDir C:\temp
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$OutputDir
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $repoRoot 'ShalazamPlugin.sln'
$modMain = Join-Path $repoRoot 'ShalazamPlugin\ModMain.cs'
$installerDir = Join-Path $PSScriptRoot 'installer'

if (-not $OutputDir) {
    $OutputDir = Join-Path $repoRoot 'artifacts'
}

# Version lives in a hand-maintained const, not the assembly metadata.
$source = Get-Content -LiteralPath $modMain -Raw
$match = [regex]::Match($source, 'PluginVersion\s*=\s*"([^"]+)"')
if (-not $match.Success) {
    throw "Could not find PluginVersion in $modMain"
}
$version = $match.Groups[1].Value
Write-Host "Packaging ShalazamPlugin $version ($Configuration)"

dotnet build $solution -c $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE"
}

$builtDll = Join-Path $repoRoot "ShalazamPlugin\bin\$Configuration\net6.0\ShalazamPlugin.dll"
if (-not (Test-Path -LiteralPath $builtDll -PathType Leaf)) {
    throw "Built assembly not found at $builtDll"
}

$staging = Join-Path ([System.IO.Path]::GetTempPath()) ("ShalazamPlugin-" + [System.IO.Path]::GetRandomFileName())
New-Item -ItemType Directory -Path $staging -Force | Out-Null

try {
    Copy-Item -LiteralPath $builtDll -Destination $staging
    foreach ($name in @('Install.cmd', 'install.ps1', 'README.txt')) {
        Copy-Item -LiteralPath (Join-Path $installerDir $name) -Destination $staging
    }

    if (-not (Test-Path -LiteralPath $OutputDir -PathType Container)) {
        New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    }

    $zipPath = Join-Path $OutputDir "ShalazamPlugin-$version.zip"
    if (Test-Path -LiteralPath $zipPath) {
        Remove-Item -LiteralPath $zipPath -Force
    }

    Compress-Archive -Path (Join-Path $staging '*') -DestinationPath $zipPath
    Write-Host "Wrote $zipPath"
    Write-Host ''
    Write-Host 'To publish:'
    Write-Host "  gh release create v$version `"$zipPath`" --title `"v$version`" --notes `"...`""
} finally {
    Remove-Item -LiteralPath $staging -Recurse -Force -ErrorAction SilentlyContinue
}
