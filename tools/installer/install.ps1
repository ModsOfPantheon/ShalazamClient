<#
.SYNOPSIS
    Installs the ShalazamPlugin MelonLoader mod into every Pantheon: Rise of the Fallen
    installation found on this machine.

.DESCRIPTION
    Locates Pantheon installs (Steam and standalone-launcher layouts), shows exactly what
    it intends to change, and only proceeds once you confirm. It writes solely into the
    game's Mods\ and UserData\ folders and never modifies a game file.

.PARAMETER GamePath
    Skip auto-detection and target this install directory (the folder containing Pantheon.exe).

.PARAMETER ApiKey
    Shalazam API key to write into UserData\MelonPreferences.cfg. Prompted for if omitted.

.PARAMETER Uninstall
    Remove the mod instead of installing it.

.PARAMETER Quiet
    Run without any prompts. Implies "yes" to the confirmation, so pair it with -GamePath.

.EXAMPLE
    .\install.ps1
    .\install.ps1 -GamePath "D:\PantheonPTR\App" -ApiKey abc123 -Quiet
    .\install.ps1 -Uninstall
#>
[CmdletBinding()]
param(
    [string]$GamePath,
    [string]$ApiKey,
    [switch]$Uninstall,
    [switch]$Quiet
)

$ErrorActionPreference = 'Stop'

$script:DllName        = 'ShalazamPlugin.dll'
$script:SourceDll      = Join-Path $PSScriptRoot $DllName
$script:PrefsSection   = 'ShalazamApi'
$script:SteamAppId     = '3107230'
$script:MelonLoaderUrl = 'https://melonwiki.xyz/#/README?id=requirements'

# ---------------------------------------------------------------------------
# Output helpers
# ---------------------------------------------------------------------------

function Write-Title($text) {
    Write-Host ''
    Write-Host "  $text" -ForegroundColor Cyan
    Write-Host "  $('-' * $text.Length)" -ForegroundColor DarkCyan
}

function Write-Step($text)  { Write-Host "      - $text" }
function Write-Warn($text)  { Write-Host "  !   $text" -ForegroundColor Yellow }
function Write-Fail($text)  { Write-Host "  X   $text" -ForegroundColor Red }
function Write-Ok($text)    { Write-Host "  OK  $text" -ForegroundColor Green }

function Get-MaskedKey([string]$key) {
    if ([string]::IsNullOrWhiteSpace($key)) {
        return '(none)'
    }
    if ($key.Length -le 10) {
        return ('*' * $key.Length)
    }
    return ($key.Substring(0, 6) + ('*' * 8) + $key.Substring($key.Length - 4))
}

# ---------------------------------------------------------------------------
# Discovery
# ---------------------------------------------------------------------------

# A directory only counts as a Pantheon install if all three of these are present.
# Pantheon.exe alone matches launcher stubs and half-deleted installs.
function Test-PantheonDir([string]$path) {
    if ([string]::IsNullOrWhiteSpace($path)) {
        return $false
    }
    if (-not (Test-Path -LiteralPath $path -PathType Container)) {
        return $false
    }
    foreach ($marker in @('Pantheon.exe', 'GameAssembly.dll', 'UnityPlayer.dll')) {
        if (-not (Test-Path -LiteralPath (Join-Path $path $marker) -PathType Leaf)) {
            return $false
        }
    }
    return $true
}

# The build doesn't stamp a meaningful assembly version, so the file date is what we
# show the user when reporting what's already installed.
function Get-InstalledModDate([string]$dll) {
    if (-not (Test-Path -LiteralPath $dll -PathType Leaf)) {
        return $null
    }
    return "dated $((Get-Item -LiteralPath $dll).LastWriteTime.ToString('yyyy-MM-dd'))"
}

function New-Candidate([string]$path, [string]$source) {
    $full = (Resolve-Path -LiteralPath $path).ProviderPath.TrimEnd('\')
    $melonDir  = Join-Path $full 'MelonLoader'
    $proxyDll  = Join-Path $full 'version.dll'
    $modDll    = Join-Path $full "Mods\$DllName"

    return [pscustomobject]@{
        Path           = $full
        Source         = $source
        IsPtr          = ($full -match 'PTR')
        HasMelonLoader = ((Test-Path -LiteralPath $melonDir -PathType Container) -and (Test-Path -LiteralPath $proxyDll -PathType Leaf))
        ModDll         = $modDll
        Installed      = (Get-InstalledModDate $modDll)
        PrefsFile      = (Join-Path $full 'UserData\MelonPreferences.cfg')
    }
}

# Registry uninstall entries left by the standalone launcher's Inno Setup installer.
# These go stale (they survive a manual move or delete), so every hit is re-validated.
function Get-CandidatePathsFromRegistry {
    $roots = @(
        'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*',
        'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*',
        'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*'
    )
    $found = @()
    foreach ($root in $roots) {
        try {
            $entries = Get-ItemProperty $root -ErrorAction Stop | Where-Object { $_.DisplayName -like '*Pantheon*' }
        } catch {
            continue
        }
        foreach ($entry in $entries) {
            $location = $entry.InstallLocation
            if ([string]::IsNullOrWhiteSpace($location)) {
                continue
            }
            $location = $location.Trim('"').TrimEnd('\')
            # The launcher installs to <root>, but the game itself lives in <root>\App.
            $found += $location
            $found += (Join-Path $location 'App')
        }
    }
    return $found
}

function Get-SteamLibraryPaths {
    $steamRoots = @()
    foreach ($key in @('HKCU:\SOFTWARE\Valve\Steam', 'HKLM:\SOFTWARE\WOW6432Node\Valve\Steam', 'HKLM:\SOFTWARE\Valve\Steam')) {
        try {
            $props = Get-ItemProperty -Path $key -ErrorAction Stop
        } catch {
            continue
        }
        foreach ($value in @($props.SteamPath, $props.InstallPath)) {
            if (-not [string]::IsNullOrWhiteSpace($value)) {
                $steamRoots += $value.Replace('/', '\').TrimEnd('\')
            }
        }
    }

    # The registry hands back the same path in different cases, so dedupe case-insensitively.
    $steamRoots = $steamRoots | Sort-Object -Unique -Property { $_.ToLowerInvariant() }

    $libraries = @()
    foreach ($root in $steamRoots) {
        $libraries += $root
        $vdf = Join-Path $root 'steamapps\libraryfolders.vdf'
        if (-not (Test-Path -LiteralPath $vdf -PathType Leaf)) {
            continue
        }
        try {
            $content = Get-Content -LiteralPath $vdf -Raw
        } catch {
            continue
        }
        foreach ($match in [regex]::Matches($content, '"path"\s+"([^"]+)"')) {
            # Paths in .vdf are backslash-escaped.
            $libraries += $match.Groups[1].Value.Replace('\\', '\').TrimEnd('\')
        }
    }
    return ($libraries | Sort-Object -Unique -Property { $_.ToLowerInvariant() })
}

# Three routes, because any one of them can miss: the app manifest for Pantheon's AppID
# (authoritative, survives a rename of the game folder), any other manifest whose
# installdir mentions Pantheon (catches PTR and other branches), and a plain glob of
# common\ (catches an install Steam has forgotten about).
function Get-InstallDirFromManifest([string]$manifestPath) {
    try {
        $text = Get-Content -LiteralPath $manifestPath -Raw
    } catch {
        return $null
    }
    $match = [regex]::Match($text, '"installdir"\s+"([^"]+)"')
    if (-not $match.Success) {
        return $null
    }
    return $match.Groups[1].Value
}

function Get-CandidatePathsFromSteam {
    $found = @()
    foreach ($library in (Get-SteamLibraryPaths)) {
        $common = Join-Path $library 'steamapps\common'
        if (Test-Path -LiteralPath $common -PathType Container) {
            try {
                $found += (Get-ChildItem -LiteralPath $common -Directory -Filter 'Pantheon*' -ErrorAction Stop | ForEach-Object { $_.FullName })
            } catch {
                # Unreadable library, nothing to do.
            }
        }

        $steamapps = Join-Path $library 'steamapps'
        if (-not (Test-Path -LiteralPath $steamapps -PathType Container)) {
            continue
        }

        $knownManifest = Join-Path $steamapps "appmanifest_$SteamAppId.acf"
        if (Test-Path -LiteralPath $knownManifest -PathType Leaf) {
            $installDir = Get-InstallDirFromManifest $knownManifest
            if (-not [string]::IsNullOrWhiteSpace($installDir)) {
                $found += (Join-Path $common $installDir)
            }
        }

        try {
            $manifests = Get-ChildItem -LiteralPath $steamapps -Filter 'appmanifest_*.acf' -File -ErrorAction Stop
        } catch {
            continue
        }
        foreach ($manifest in $manifests) {
            $installDir = Get-InstallDirFromManifest $manifest.FullName
            if (-not [string]::IsNullOrWhiteSpace($installDir) -and $installDir -like '*Pantheon*') {
                $found += (Join-Path $common $installDir)
            }
        }
    }
    return $found
}

# A shortlist of plausible locations on every fixed drive. Deliberately shallow -
# a full disk crawl would take minutes and is never worth it.
function Get-CandidatePathsFromDrives {
    $found = @()
    $drives = [System.IO.DriveInfo]::GetDrives() | Where-Object { $_.DriveType -eq 'Fixed' -and $_.IsReady }

    foreach ($drive in $drives) {
        $root = $drive.RootDirectory.FullName

        foreach ($relative in @('SteamLibrary\steamapps\common', 'Games', 'Program Files (x86)\Steam\steamapps\common')) {
            $parent = Join-Path $root $relative
            if (-not (Test-Path -LiteralPath $parent -PathType Container)) {
                continue
            }
            try {
                $found += (Get-ChildItem -LiteralPath $parent -Directory -Filter 'Pantheon*' -ErrorAction Stop | ForEach-Object { $_.FullName })
            } catch {
                continue
            }
        }

        try {
            $topLevel = Get-ChildItem -LiteralPath $root -Directory -Filter 'Pantheon*' -ErrorAction Stop
        } catch {
            continue
        }
        foreach ($dir in $topLevel) {
            $found += $dir.FullName
            $found += (Join-Path $dir.FullName 'App')
        }
    }
    return $found
}

function Find-PantheonInstalls {
    $sources = [ordered]@{
        'launcher registry' = { Get-CandidatePathsFromRegistry }
        'Steam'             = { Get-CandidatePathsFromSteam }
        'disk scan'         = { Get-CandidatePathsFromDrives }
    }

    $seen = @{}
    $results = @()

    foreach ($name in $sources.Keys) {
        $paths = @()
        try {
            $paths = & $sources[$name]
        } catch {
            Write-Warn "Could not search $name`: $($_.Exception.Message)"
            continue
        }
        foreach ($path in $paths) {
            if (-not (Test-PantheonDir $path)) {
                continue
            }
            $key = (Resolve-Path -LiteralPath $path).ProviderPath.TrimEnd('\').ToLowerInvariant()
            if ($seen.ContainsKey($key)) {
                continue
            }
            $seen[$key] = $true
            $results += (New-Candidate $path $name)
        }
    }

    return $results
}

# ---------------------------------------------------------------------------
# MelonPreferences.cfg
# ---------------------------------------------------------------------------

function Get-ExistingApiKey([string]$prefsFile) {
    if (-not (Test-Path -LiteralPath $prefsFile -PathType Leaf)) {
        return $null
    }
    $lines = @(Get-Content -LiteralPath $prefsFile)
    $inSection = $false
    foreach ($line in $lines) {
        $trimmed = $line.Trim()
        if ($trimmed -match '^\[(.+)\]$') {
            $inSection = ($Matches[1] -eq $PrefsSection)
            continue
        }
        if ($inSection -and $trimmed -match '^ApiKey\s*=\s*"?([^"]*)"?\s*$') {
            return $Matches[1]
        }
    }
    return $null
}

# Surgical, line-based edit. This file is shared with every other MelonLoader mod the
# user has installed, so it is never reserialised - only the one ApiKey line changes.
function Set-ApiKeyInPrefs([string]$prefsFile, [string]$key) {
    $userData = Split-Path -Parent $prefsFile
    if (-not (Test-Path -LiteralPath $userData -PathType Container)) {
        New-Item -ItemType Directory -Path $userData -Force | Out-Null
    }

    $lines = @()
    if (Test-Path -LiteralPath $prefsFile -PathType Leaf) {
        $lines = @(Get-Content -LiteralPath $prefsFile)
    }

    $keyLine = "ApiKey = `"$key`""
    $sectionIndex = -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i].Trim() -eq "[$PrefsSection]") {
            $sectionIndex = $i
            break
        }
    }

    if ($sectionIndex -lt 0) {
        $output = New-Object System.Collections.Generic.List[string]
        $output.AddRange([string[]]$lines)
        if ($output.Count -gt 0 -and -not [string]::IsNullOrWhiteSpace($output[$output.Count - 1])) {
            $output.Add('')
        }
        $output.Add("[$PrefsSection]")
        $output.Add($keyLine)
        $lines = $output.ToArray()
    } else {
        $replaced = $false
        for ($i = $sectionIndex + 1; $i -lt $lines.Count; $i++) {
            if ($lines[$i].Trim() -match '^\[.+\]$') {
                break
            }
            if ($lines[$i].Trim() -match '^ApiKey\s*=') {
                $lines[$i] = $keyLine
                $replaced = $true
                break
            }
        }
        if (-not $replaced) {
            $output = New-Object System.Collections.Generic.List[string]
            $output.AddRange([string[]]$lines)
            $output.Insert($sectionIndex + 1, $keyLine)
            $lines = $output.ToArray()
        }
    }

    Write-PrefsFile $prefsFile $lines
}

function Remove-ApiKeySection([string]$prefsFile) {
    if (-not (Test-Path -LiteralPath $prefsFile -PathType Leaf)) {
        return $false
    }
    $lines = @(Get-Content -LiteralPath $prefsFile)
    $output = New-Object System.Collections.Generic.List[string]
    $inSection = $false
    $removed = $false

    foreach ($line in $lines) {
        $trimmed = $line.Trim()
        if ($trimmed -match '^\[(.+)\]$') {
            $inSection = ($Matches[1] -eq $PrefsSection)
            if ($inSection) {
                $removed = $true
                continue
            }
        }
        if (-not $inSection) {
            $output.Add($line)
        }
    }

    if ($removed) {
        Write-PrefsFile $prefsFile $output.ToArray()
    }
    return $removed
}

# MelonLoader's config parser chokes on a UTF-8 BOM, which is exactly what
# Set-Content -Encoding utf8 writes on Windows PowerShell 5.1.
function Write-PrefsFile([string]$prefsFile, [string[]]$lines) {
    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllLines($prefsFile, $lines, $encoding)
}

# ---------------------------------------------------------------------------
# Prompts
# ---------------------------------------------------------------------------

function Confirm-Plan {
    if ($Quiet) {
        return 'all'
    }
    while ($true) {
        Write-Host ''
        Write-Host '  Proceed?  [Y] Yes, apply to all   [S] Select individually   [N] Cancel : ' -ForegroundColor White -NoNewline
        $answer = (Read-Host).Trim().ToLowerInvariant()
        switch ($answer) {
            { $_ -in @('y', 'yes') }    { return 'all' }
            { $_ -in @('s', 'select') }  { return 'select' }
            { $_ -in @('n', 'no', 'q') } { return 'cancel' }
            default { Write-Warn 'Please answer Y, S or N.' }
        }
    }
}

function Select-Targets($targets) {
    $chosen = @()
    foreach ($target in $targets) {
        Write-Host ''
        Write-Host "  Apply to $($target.Path)? [y/N] : " -NoNewline
        $answer = (Read-Host).Trim().ToLowerInvariant()
        if ($answer -in @('y', 'yes')) {
            $chosen += $target
        }
    }
    return $chosen
}

function Read-ApiKey($targets) {
    if ($Quiet) {
        return $null
    }

    $existing = @($targets | ForEach-Object { Get-ExistingApiKey $_.PrefsFile } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    Write-Host ''
    if ($existing.Count -gt 0) {
        Write-Host "  An API key is already configured ($(Get-MaskedKey $existing[0]))."
        Write-Host '  Press Enter to keep it, or paste a new key to replace it.'
    } else {
        Write-Host '  Enter your Shalazam API key (from your profile on https://shalazam.info).'
        Write-Host '  Press Enter to skip - you can add it to MelonPreferences.cfg later.'
    }

    while ($true) {
        Write-Host '  API key: ' -NoNewline
        $key = (Read-Host).Trim()
        if ([string]::IsNullOrWhiteSpace($key)) {
            return $null
        }
        if ($key -match '["\s]') {
            Write-Warn 'That key contains quotes or spaces. Paste the key on its own.'
            continue
        }
        if ($key -notmatch '^[0-9a-fA-F]{16,}$') {
            Write-Warn "That doesn't look like a Shalazam API key (expected a long hex string)."
            Write-Host '  Use it anyway? [y/N] : ' -NoNewline
            $confirm = (Read-Host).Trim().ToLowerInvariant()
            if ($confirm -notin @('y', 'yes')) {
                continue
            }
        }
        return $key
    }
}

function Wait-ForKeyPress {
    if ($Quiet) {
        return
    }
    Write-Host ''
    Write-Host '  Press any key to close...' -ForegroundColor DarkGray
    # With stdin piped in there is no keyboard to wait for, and ReadKey blocks forever
    # rather than throwing, so bail out before trying it.
    if ([Console]::IsInputRedirected) {
        return
    }
    try {
        $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown') | Out-Null
    } catch {
        # Host doesn't support raw key reads (ISE, for example) - fall back.
        Read-Host | Out-Null
    }
}

# ---------------------------------------------------------------------------
# Guards
# ---------------------------------------------------------------------------

function Test-GameRunning {
    $processes = @(Get-Process -Name 'Pantheon' -ErrorAction SilentlyContinue)
    return ($processes.Count -gt 0)
}

function Test-Writable([string]$directory) {
    $probe = Join-Path $directory (".shalazam-write-test-" + [System.IO.Path]::GetRandomFileName())
    try {
        [System.IO.File]::WriteAllText($probe, '')
        Remove-Item -LiteralPath $probe -Force -ErrorAction SilentlyContinue
        return $true
    } catch {
        return $false
    }
}

function Test-IsElevated {
    $identity = [System.Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object System.Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)
}

# ---------------------------------------------------------------------------
# Plan + apply
# ---------------------------------------------------------------------------

function Show-Plan($targets, $skipped, [string]$key, [bool]$writeKey) {
    Write-Title 'The following changes will be made'

    $index = 0
    foreach ($target in $targets) {
        $index++
        $labels = @($target.Source)
        if ($target.IsPtr) {
            $labels += 'PTR'
        }
        Write-Host ''
        Write-Host "  [$index] $($target.Path)" -ForegroundColor White
        Write-Host "      found via $($labels -join ', ')" -ForegroundColor DarkGray

        if ($null -eq $target.Installed) {
            Write-Step "Copy $DllName -> $($target.ModDll)  (new)"
        } else {
            Write-Step "Copy $DllName -> $($target.ModDll)  (replaces $($target.Installed))"
        }

        if ($writeKey) {
            $current = Get-ExistingApiKey $target.PrefsFile
            Write-Step "Set ApiKey in $($target.PrefsFile)  (currently $(Get-MaskedKey $current))"
        }
    }

    if ($skipped.Count -gt 0) {
        Write-Host ''
        foreach ($skip in $skipped) {
            Write-Warn "Skipping $($skip.Path) - $($skip.Reason)"
        }
    }

    Write-Host ''
    Write-Host '  Nothing outside those Mods\ and UserData\ paths is touched.' -ForegroundColor DarkGray
}

function Invoke-Install($target, [string]$key) {
    $result = [pscustomobject]@{ Path = $target.Path; Actions = @(); Errors = @() }
    $modsDir = Join-Path $target.Path 'Mods'

    try {
        if (-not (Test-Path -LiteralPath $modsDir -PathType Container)) {
            New-Item -ItemType Directory -Path $modsDir -Force | Out-Null
            $result.Actions += 'Created Mods folder'
        }
        Copy-Item -LiteralPath $SourceDll -Destination $target.ModDll -Force
        if ($null -eq $target.Installed) {
            $result.Actions += "Installed $DllName"
        } else {
            $result.Actions += "Updated $DllName (was $($target.Installed))"
        }
    } catch {
        $result.Errors += "Could not copy $DllName`: $($_.Exception.Message)"
        return $result
    }

    if (-not [string]::IsNullOrWhiteSpace($key)) {
        try {
            Set-ApiKeyInPrefs $target.PrefsFile $key
            $result.Actions += "Set ApiKey to $(Get-MaskedKey $key)"
        } catch {
            $result.Errors += "Could not write API key: $($_.Exception.Message)"
        }
    }

    return $result
}

function Invoke-Uninstall($target, [bool]$removeSettings) {
    $result = [pscustomobject]@{ Path = $target.Path; Actions = @(); Errors = @() }

    try {
        Remove-Item -LiteralPath $target.ModDll -Force
        $result.Actions += "Removed $DllName"
    } catch {
        $result.Errors += "Could not remove $DllName`: $($_.Exception.Message)"
    }

    if ($removeSettings) {
        try {
            if (Remove-ApiKeySection $target.PrefsFile) {
                $result.Actions += "Removed [$PrefsSection] settings"
            }
        } catch {
            $result.Errors += "Could not clean MelonPreferences.cfg: $($_.Exception.Message)"
        }
    }

    return $result
}

function Show-Summary($results, [string]$verb) {
    Write-Title 'Done'

    if ($results.Count -eq 0) {
        Write-Host '  Nothing was changed.'
        return
    }

    $failed = 0
    foreach ($result in $results) {
        Write-Host ''
        Write-Host "  $($result.Path)" -ForegroundColor White
        foreach ($action in $result.Actions) {
            Write-Ok $action
        }
        foreach ($error in $result.Errors) {
            Write-Fail $error
            $failed++
        }
    }

    Write-Host ''
    if ($failed -gt 0) {
        Write-Warn "$verb completed with $failed problem(s) - see above."
    } elseif ($verb -eq 'Install') {
        Write-Host '  Launch Pantheon as normal. Check the MelonLoader console for "ShalazamPlugin".' -ForegroundColor Green
    } else {
        Write-Host '  The mod has been removed.' -ForegroundColor Green
    }
}

# ---------------------------------------------------------------------------
# Modes
# ---------------------------------------------------------------------------

function Get-Installs {
    if ($GamePath) {
        if (-not (Test-PantheonDir $GamePath)) {
            Write-Fail "$GamePath doesn't look like a Pantheon install (expected Pantheon.exe, GameAssembly.dll and UnityPlayer.dll)."
            return @()
        }
        return @(New-Candidate $GamePath 'command line')
    }

    Write-Host ''
    Write-Host '  Searching for Pantheon installations...' -ForegroundColor DarkGray
    $installs = @(Find-PantheonInstalls)

    if ($installs.Count -eq 0 -and -not $Quiet) {
        Write-Host ''
        Write-Warn 'No Pantheon installation found automatically.'
        Write-Host '  Enter the folder containing Pantheon.exe, or press Enter to give up.'
        Write-Host '  Path: ' -NoNewline
        $manual = (Read-Host).Trim().Trim('"')
        if (-not [string]::IsNullOrWhiteSpace($manual)) {
            if (Test-PantheonDir $manual) {
                $installs = @(New-Candidate $manual 'entered by hand')
            } else {
                Write-Fail "$manual doesn't contain Pantheon.exe, GameAssembly.dll and UnityPlayer.dll."
            }
        }
    }

    return $installs
}

function Invoke-InstallMode {
    if (-not (Test-Path -LiteralPath $SourceDll -PathType Leaf)) {
        Write-Fail "$DllName is missing from $PSScriptRoot."
        Write-Host "  Put $DllName in that folder, next to this script, and run it again."
        return
    }

    $installs = @(Get-Installs)
    if ($installs.Count -eq 0) {
        return
    }

    $targets = @()
    $skipped = @()
    foreach ($install in $installs) {
        if (-not $install.HasMelonLoader) {
            $skipped += [pscustomobject]@{ Path = $install.Path; Reason = "MelonLoader isn't installed here ($MelonLoaderUrl)" }
            continue
        }
        if (-not (Test-Writable $install.Path)) {
            $hint = 'the folder is read-only'
            if (-not (Test-IsElevated)) {
                $hint = 'no write access - right-click Install.cmd and choose "Run as administrator"'
            }
            $skipped += [pscustomobject]@{ Path = $install.Path; Reason = $hint }
            continue
        }
        $targets += $install
    }

    if ($targets.Count -eq 0) {
        Write-Host ''
        foreach ($skip in $skipped) {
            Write-Warn "Skipping $($skip.Path) - $($skip.Reason)"
        }
        Write-Host ''
        Write-Fail 'No installation is ready for the mod.'
        return
    }

    $key = $ApiKey
    if ([string]::IsNullOrWhiteSpace($key)) {
        $key = Read-ApiKey $targets
    }
    $writeKey = -not [string]::IsNullOrWhiteSpace($key)

    Show-Plan $targets $skipped $key $writeKey

    $choice = Confirm-Plan
    if ($choice -eq 'cancel') {
        Write-Host ''
        Write-Host '  Cancelled. Nothing was changed.' -ForegroundColor Yellow
        return
    }
    if ($choice -eq 'select') {
        $targets = @(Select-Targets $targets)
        if ($targets.Count -eq 0) {
            Write-Host ''
            Write-Host '  Nothing selected. Nothing was changed.' -ForegroundColor Yellow
            return
        }
    }

    $results = @()
    foreach ($target in $targets) {
        $results += (Invoke-Install $target $key)
    }
    Show-Summary $results 'Install'
}

function Invoke-UninstallMode {
    $installs = @(Get-Installs | Where-Object { $null -ne $_.Installed })
    if ($installs.Count -eq 0) {
        Write-Host ''
        Write-Warn "$DllName isn't installed in any Pantheon installation found."
        return
    }

    $removeSettings = $false
    if (-not $Quiet) {
        Write-Host ''
        Write-Host '  Also remove your API key and settings from MelonPreferences.cfg? [y/N] : ' -NoNewline
        $answer = (Read-Host).Trim().ToLowerInvariant()
        $removeSettings = ($answer -in @('y', 'yes'))
    }

    Write-Title 'The following changes will be made'
    foreach ($install in $installs) {
        Write-Host ''
        Write-Host "  $($install.Path)" -ForegroundColor White
        Write-Step "Delete $($install.ModDll)  ($($install.Installed))"
        if ($removeSettings) {
            Write-Step "Remove the [$PrefsSection] section from $($install.PrefsFile)"
        }
    }
    Write-Host ''
    Write-Host '  Nothing else is touched. MelonLoader and other mods are left alone.' -ForegroundColor DarkGray

    $choice = Confirm-Plan
    if ($choice -eq 'cancel') {
        Write-Host ''
        Write-Host '  Cancelled. Nothing was changed.' -ForegroundColor Yellow
        return
    }
    if ($choice -eq 'select') {
        $installs = @(Select-Targets $installs)
        if ($installs.Count -eq 0) {
            Write-Host ''
            Write-Host '  Nothing selected. Nothing was changed.' -ForegroundColor Yellow
            return
        }
    }

    $results = @()
    foreach ($install in $installs) {
        $results += (Invoke-Uninstall $install $removeSettings)
    }
    Show-Summary $results 'Uninstall'
}

function Show-Menu {
    Write-Host ''
    Write-Host '  What would you like to do?'
    Write-Host ''
    Write-Host '    1) Install or update ShalazamPlugin'
    Write-Host '    2) Uninstall ShalazamPlugin'
    Write-Host '    3) Exit'
    Write-Host ''

    while ($true) {
        Write-Host '  Choice [1] : ' -NoNewline
        $answer = (Read-Host).Trim()
        if ([string]::IsNullOrWhiteSpace($answer)) {
            $answer = '1'
        }
        switch ($answer) {
            '1' { return 'install' }
            '2' { return 'uninstall' }
            '3' { return 'exit' }
            default { Write-Warn 'Enter 1, 2 or 3.' }
        }
    }
}

# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

Write-Host ''
Write-Host '  ShalazamPlugin installer' -ForegroundColor Cyan
Write-Host '  Uploads Pantheon world data to shalazam.info' -ForegroundColor DarkGray

try {
    if (Test-GameRunning) {
        Write-Host ''
        Write-Fail 'Pantheon is running. Close the game first - its mod files are locked while it runs.'
        Wait-ForKeyPress
        exit 1
    }

    $mode = 'install'
    if ($Uninstall) {
        $mode = 'uninstall'
    } elseif (-not $Quiet) {
        $mode = Show-Menu
    }

    switch ($mode) {
        'install'   { Invoke-InstallMode }
        'uninstall' { Invoke-UninstallMode }
        'exit'      { Write-Host ''; Write-Host '  Nothing was changed.' }
    }
} catch {
    Write-Host ''
    Write-Fail "Unexpected error: $($_.Exception.Message)"
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
}

Wait-ForKeyPress
