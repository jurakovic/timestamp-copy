[CmdletBinding(PositionalBinding=$false)]
Param(
    [switch][Alias('h')]$Help,
    [switch][Alias('v')]$Version,
    [switch][Alias('i')]$Install,
    [switch][Alias('b')]$InstallBackgroundMode,
    [switch][Alias('u')]$Uninstall,
    [string][Alias('c')]$Copy,
    [string][Alias('p')]$Paste,
    [string][Alias('pc')]$PasteDateCreated,
    [string][Alias('pm')]$PasteDateModified,
    [switch][Alias('z')]$Undo,
    [switch][Alias('q')]$Quiet,
    [switch][Alias('y')]$SkipConfirm,
    [ValidateSet("Terminal","Standalone","Background")][string][Alias('m')]$ScriptMode = "Terminal"
)

##### Constants

$homepage = "https://github.com/jurakovic/timestamp-copy"
$versionn = "2.1.0"
$scriptPath = "$PSCommandPath"
$exePath = "$PSScriptRoot\tscp.exe"
$iconPath = "$PSScriptRoot\tscp.ico"
$appdataPath = "$env:LOCALAPPDATA\TimestampCopy"
$clipPath = "$appdataPath\clip"
$undoPath = "$appdataPath\clip-undo"
$fRootKey = "HKEY_CLASSES_ROOT\*\shell\TimestampCopy"
$dRootKey = "HKEY_CLASSES_ROOT\Directory\shell\TimestampCopy"
$lRootKey = "HKEY_CLASSES_ROOT\lnkfile\shell\TimestampCopy"
$datetimeFormat = "yyyy-MM-dd HH:mm:ss"

##### Main

function Main {
    if ($Help) {
        Show-Help
        exit 0
    }

    if ($Version) {
        Write-Host "$versionn"
        exit 0
    }

    if ($(@($Install, $InstallBackgroundMode, $Uninstall | Where-Object { $_ }).Count) -ge 2) {
        Show-Guard-Message "Parameters -Install, -InstallBackgroundMode, -Uninstall cannot be used together."
    }

    if ($Install) {
        Install -ScriptMode "Standalone"
        exit 0
    }

    if ($InstallBackgroundMode) {
        Install -ScriptMode "Background"
        exit 0
    }

    if ($Uninstall) {
        Uninstall
        exit 0
    }

    if ($(@($Copy, $Paste, $PasteDateCreated, $PasteDateModified, $Undo | Where-Object { $_ }).Count) -ge 2) {
        Show-Guard-Message "Parameters -Copy, -Paste, -PasteDateCreated, -PasteDateModified, -Undo cannot be used together."
    }

    if ($Copy) {
        Copy-Timestamps -FilePath "$Copy"
        Pause-Script
        exit 0
    }

    if ($Paste) {
        Paste-Timestamps -FilePath "$Paste"
        Pause-Script
        exit 0
    }

    if ($PasteDateCreated) {
        Paste-DateCreated -FilePath "$PasteDateCreated"
        Pause-Script
        exit 0
    }

    if ($PasteDateModified) {
        Paste-DateModified -FilePath "$PasteDateModified"
        Pause-Script
        exit 0
    }

    if ($Undo) {
        Undo-Timestamps
        Pause-Script
        exit 0
    }

    $ScriptMode = "Standalone" # to show "Pause-Script" message to prevent closing the window
    Show-Menu
}

##### Help

function Show-Help {
    Write-Host "TimestampCopy v$versionn"
    Write-Host ""
    Write-Host "Parameters:"
    Write-Host "  -Help (-h)                       Print help."
    Write-Host "  -Version (-v)                    Print the current version of the script."
    Write-Host "  -Install (-i)                    Install the context menu entries for the script in standalone mode."
    Write-Host "  -InstallBackgroundMode (-b)      Install the context menu entries for the script in background mode (runs without a terminal window)."
    Write-Host "  -Uninstall (-u)                  Uninstall the context menu entries and remove related data."
    Write-Host "  -Copy (-c) <path>                Copy timestamps of the specified file or folder to the clipboard."
    Write-Host "  -Paste (-p) <path>               Paste the copied timestamps to the specified file or folder."
    Write-Host "  -PasteDateCreated (-pc) <path>   Paste only the copied Date Created timestamp to the specified file or folder."
    Write-Host "  -PasteDateModified (-pm) <path>  Paste only the copied Date Modified timestamp to the specified file or folder."
    Write-Host "  -Undo (-z)                       Restore the previous timestamps of the last modified file or folder."
    Write-Host "  -Quiet (-q)                      Suppress output messages. After run check $LastExitCode or $? for exit code."
    Write-Host "  -SkipConfirm (-y)                Skip confirmation prompts when applying changes."
    Write-Host "  *none*                           Show the install/uninstall menu."
    Write-Host ""
    Write-Host "Some examples:"
    Write-Host "# Install the context menu entries"
    Write-Host ".\TimestampCopy.ps1 -i"
    Write-Host ""
    Write-Host "# Copy timestamps"
    Write-Host ".\TimestampCopy.ps1 -c `"C:\Foo.txt`""
    Write-Host ""
    Write-Host "# Paste timestamps"
    Write-Host ".\TimestampCopy.ps1 -p `"D:\Bar.txt`""
    Write-Host ""
    Write-Host "# Paste timestamps without output messages (confirm prompt still shown)"
    Write-Host ".\TimestampCopy.ps1 -p `"D:\Bar.txt`" -q"
    Write-Host ""
    Write-Host "# Paste timestamps without output messages and confirm prompt"
    Write-Host ".\TimestampCopy.ps1 -p `"D:\Bar.txt`" -q -y"
    Write-Host ""
    Write-Host "# Paste Date Created"
    Write-Host ".\TimestampCopy.ps1 -pc `"D:\Bar.txt`""
    Write-Host ""
    Write-Host "# Paste Date Modified"
    Write-Host ".\TimestampCopy.ps1 -pm `"D:\Bar.txt`""
    Write-Host ""
    Write-Host "# Undo"
    Write-Host ".\TimestampCopy.ps1 -z"
    Write-Host ""
    Write-Host "For more information, visit https://github.com/jurakovic/timestamp-copy"
}

##### Install/Uninstall

function Show-Menu {
    while ($true) {
        Clear-Host
        Write-Host ""
        Write-Host "  Timestamp Copy ($versionn)    "
        Write-Host "                                "
        Write-Host "  [i] Install                   "
        Write-Host "  [b] Install (Background Mode) "
        Write-Host "  [u] Uninstall                 "
        Write-Host "  [h] Help                      "
        Write-Host "                                "
        Write-Host "  [q] Quit                      "
        Write-Host ""
        $option = Read-Host "Choose option"

        Clear-Host
        switch ($option) {
            "i" { Install -ScriptMode "Standalone" }
            "b" { Install -ScriptMode "Background" }
            "u" { Uninstall }
            "h" { Show-Help }
            "q" { exit 0 }
            default { Write-Host "Unknown option: $option" }
        }

        Pause-Script "continue"
    }
}

function Pause-Script {
    param (
        [string]$Option = "exit"
    )

    if ($ScriptMode -ieq "Standalone") {
        Write-Host -NoNewLine "Press any key to $Option..."
        $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
    }
}

function Install {
    param (
        [string]$ScriptMode
    )

    net session *> $null
    if (-Not $?) {
        Write-Host "Administrator privileges required. Run as Administrator." -ForegroundColor Red
        Pause-Script
        exit 1
    }

    $mode = If ($ScriptMode -ieq "Background") { " (Background Mode)" } Else { "" }
    Write-Host "Installing$mode..."
    Create-AppData
    Add-ContextMenu -RootKey "$fRootKey" -ScriptMode "$ScriptMode"
    Add-ContextMenu -RootKey "$dRootKey" -ScriptMode "$ScriptMode"
    Add-ContextMenu -RootKey "$lRootKey" -ScriptMode "$ScriptMode"
    Write-Host "Done" -ForegroundColor Green
}

function Create-AppData {
    New-Item -Path "$appdataPath" -ItemType Directory -Force | Out-Null
}

function Add-ContextMenu {
    param (
        [string]$RootKey,
        [string]$ScriptMode
    )

    Add-MenuRoot -Key "$RootKey" -Label "Timestamp Copy" -IconPath "$iconPath"
    Add-MenuItem -Key "$RootKey\shell\010-Copy" -Label "Copy" -Action "Copy '%1'" -ScriptMode "$ScriptMode" -ExeFlag "-c"
    Add-MenuItem -Key "$RootKey\shell\020-Paste" -Label "Paste" -Action "Paste '%1'" -ScriptMode "$ScriptMode" -ExeFlag "-p"
    Add-MenuItem -Key "$RootKey\shell\030-PasteDateCreated" -Label "Paste \""Date Created\""" -Action "PasteDateCreated '%1'" -ScriptMode "$ScriptMode" -ExeFlag "-pc"
    Add-MenuItem -Key "$RootKey\shell\040-PasteDateModified" -Label "Paste \""Date Modified\""" -Action "PasteDateModified '%1'" -ScriptMode "$ScriptMode" -ExeFlag "-pm"
    Add-MenuItem -Key "$RootKey\shell\050-Undo" -Label "Undo" -Action "Undo" -ScriptMode "$ScriptMode"
}

function Add-MenuRoot {
    param (
        [string]$Key,
        [string]$Label,
        [string]$IconPath
    )

    reg.exe add "$Key" /v MUIVerb /d "$Label" /f | Out-Null
    reg.exe add "$Key" /v SubCommands /f | Out-Null
    reg.exe add "$Key" /v Icon /d "$IconPath" /f | Out-Null
}

function Add-MenuItem {
    param (
        [string]$Key,
        [string]$Label,
        [string]$Action,
        [string]$ScriptMode,
        [string]$ExeFlag
    )

    $headless = if ($ScriptMode -ieq "Background") { "conhost.exe --headless " } else { "" }

    # Temporary migration rule (see PLAN.md, step 1): actions already ported to tscp.exe use it
    # when the exe sits next to the script. Background mode stays on PowerShell until tscpw.exe
    # exists (step 4), because tscp.exe cannot show the message box guards.
    # Note the quoting change: '%1' (PowerShell single quotes) becomes "%1".
    $useExe = $ExeFlag -and ($ScriptMode -ine "Background") -and (Test-Path -Path "$exePath")

    $command = if ($useExe) {
        "\""$exePath\"" -m $ScriptMode $ExeFlag \""%1\"""
    } else {
        "${headless}powershell -ExecutionPolicy ByPass -NoProfile -Command \""& '$scriptPath' -ScriptMode '$ScriptMode' -$Action\"""
    }

    reg.exe add "$Key" /ve /d "$Label" /f | Out-Null
    reg.exe add "$Key\command" /ve /d "$command" /f | Out-Null
}

function Uninstall {
    Write-Host "Uninstalling..."
    Remove-ContextMenu -RootKey "$fRootKey"
    Remove-ContextMenu -RootKey "$dRootKey"
    Remove-ContextMenu -RootKey "$lRootKey"
    Remove-Item -Recurse -Force -Path "$appdataPath" *> $null
    Write-Host "Done" -ForegroundColor Green
}

function Remove-ContextMenu {
    param (
        [string]$RootKey
    )

    reg.exe delete "$RootKey" /f *> $null
}

##### Context Menu Commands (Copy/Paste Actions)

function Copy-Timestamps {
    param (
        [string]$FilePath
    )

    Path-Exists $FilePath

    $item = Get-Item -Path "$FilePath"
    $dc = $item.CreationTime.ToString("$datetimeFormat")
    $dm = $item.LastWriteTime.ToString("$datetimeFormat")

    Set-Clipboard-Content -Path "$clipPath" -Value "$dc`n$dm"

    if (-Not $Quiet) {
        Write-Host "File/Folder:   $FilePath"
        Write-Host "---"
        Write-Host "Date Created:  $dc"
        Write-Host "Date Modified: $dm"
        Write-Host "---"
        Write-Host "Timestamps copied" -ForegroundColor Green
    }
}

function Paste-Timestamps {
    param (
        [string]$FilePath
    )

    Path-Exists $FilePath
    Guard-Clipboard

    $timestamps = Get-Clipboard-Content -Path "$clipPath"
    $dcNew = $timestamps[0]
    $dmNew = $timestamps[1]

    $item = Get-Item -Path "$FilePath"
    $dcOld = $item.CreationTime.ToString("$datetimeFormat")
    $dmOld = $item.LastWriteTime.ToString("$datetimeFormat")

    Paste-Timestamps-Internal "$FilePath" "$dcOld" "$dcNew" "$dmOld" "$dmNew"
}

function Paste-DateCreated {
    param (
        [string]$FilePath
    )

    Path-Exists $FilePath
    Guard-Clipboard

    $timestamps = Get-Clipboard-Content -Path "$clipPath"
    $dcNew = $timestamps[0]

    $item = Get-Item -Path "$FilePath"
    $dcOld = $item.CreationTime.ToString("$datetimeFormat")
    $dmOld = $item.LastWriteTime.ToString("$datetimeFormat")

    Paste-Timestamps-Internal "$FilePath" "$dcOld" "$dcNew" "$dmOld" "$dmOld" # We're using here "$dmOld" two times
}

function Paste-DateModified {
    param (
        [string]$FilePath
    )

    Path-Exists $FilePath
    Guard-Clipboard

    $timestamps = Get-Clipboard-Content -Path "$clipPath"
    $dmNew = $timestamps[1]

    $item = Get-Item -Path "$FilePath"
    $dcOld = $item.CreationTime.ToString("$datetimeFormat")
    $dmOld = $item.LastWriteTime.ToString("$datetimeFormat")

    Paste-Timestamps-Internal "$FilePath" "$dcOld" "$dcOld" "$dmOld" "$dmNew" # We're using here "$dcOld" two times
}

function Paste-Timestamps-Internal {
    param (
        [string]$FilePath,
        [string]$dcOld,
        [string]$dcNew,
        [string]$dmOld,
        [string]$dmNew
    )

    if (-Not $Quiet) {
        Write-Host "File/Folder:   $FilePath"
        Write-Host "---"
        Highlight-Diff -Label "Date Created: " -Old "$dcOld" -New "$dcNew"
        Write-Host "---"
        Highlight-Diff -Label "Date Modified:" -Old "$dmOld" -New "$dmNew"
        Write-Host "---"
    }

    $applyChanges = if ($SkipConfirm -or $ScriptMode -ieq "Background") { "y" } else { Read-Host "Apply changes? (y/N)" }

    if ($applyChanges -ieq "y") {
        try {
            $item = Get-Item -Path "$FilePath"
            # Changing both values triggers "Refresh" in Windows File Explorer
            $item.CreationTime = [datetime]::ParseExact("$dcNew", "$datetimeFormat", $null)
            $item.LastWriteTime = [datetime]::ParseExact("$dmNew", "$datetimeFormat", $null)
        }
        catch {
            Write-Host "Error" -ForegroundColor Red
            Show-Guard-Message "$Error"
        }

        Set-Clipboard-Content -Path "$undoPath" -Value "$FilePath`n$dcOld`n$dmOld" # Backup old timestamps

        if (-Not $Quiet) {
            Write-Host "Done" -ForegroundColor Green
        }
    } elseif (-Not $Quiet) {
        Write-Host "Canceled" -ForegroundColor Yellow
    }
}

function Undo-Timestamps {
    Guard-Undo-Clipboard

    $timestamps = Get-Clipboard-Content -Path "$undoPath"
    $filePath = $timestamps[0]
    $dcNew = $timestamps[1]
    $dmNew = $timestamps[2]

    $item = Get-Item -Path "$filePath"
    $dcOld = $item.CreationTime.ToString("$datetimeFormat")
    $dmOld = $item.LastWriteTime.ToString("$datetimeFormat")

    Paste-Timestamps-Internal "$filePath" "$dcOld" "$dcNew" "$dmOld" "$dmNew"
}

##### Helper Functions

function Set-Clipboard-Content {
    param (
        [string]$Path,
        [string]$Value
    )

    $encoded  = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes("$Value"))
    Set-Content -Path "$Path" -Value "$encoded"
}

function Get-Clipboard-Content {
    param (
        [string]$Path
    )

    $encoded = Get-Content -Path "$Path"
    $decoded = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($encoded))
    return $decoded -split "`n"
}

function Path-Exists {
    param (
        [string]$FilePath
    )

    if (-Not (Test-Path -Path "$FilePath")) {
        Show-Guard-Message "Path '$FilePath' does not exist."
    }
}

function Guard-Clipboard {
    if (-Not (Test-Path -Path "$clipPath")) {
        Show-Guard-Message "Timestamps clipboard empty. Copy new timestamps.    "
    }

    try {
        $encoded = Get-Content -Path "$clipPath"
        [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($encoded)) | Out-Null
    } catch {
        Show-Guard-Message "Timestamps clipboard corrupted. Copy new timestamps."
    }

    $timestamps = Get-Clipboard-Content -Path "$clipPath"
    if ($timestamps.Count -ne 2) {
        Show-Guard-Message "Timestamps clipboard corrupted. Copy new timestamps."
    }

    try {
        [datetime]::ParseExact($timestamps[0], "$datetimeFormat", $null) | Out-Null
        [datetime]::ParseExact($timestamps[1], "$datetimeFormat", $null) | Out-Null
    } catch {
        Show-Guard-Message "Timestamps clipboard corrupted. Copy new timestamps."
    }
}

function Guard-Undo-Clipboard {
    if (-Not (Test-Path -Path "$undoPath")) {
        Show-Guard-Message "Timestamps undo clipboard empty. Paste some timestamps.   "
    }

    try {
        $encoded = Get-Content -Path "$undoPath"
        [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($encoded)) | Out-Null
    } catch {
        Show-Guard-Message "Timestamps undo clipboard corrupted. Paste new timestamps."
    }

    $timestamps = Get-Clipboard-Content -Path "$undoPath"
    if ($timestamps.Count -ne 3) {
        Show-Guard-Message "Timestamps undo clipboard corrupted. Paste new timestamps."
    }

    try {
        [datetime]::ParseExact($timestamps[1], "$datetimeFormat", $null) | Out-Null
        [datetime]::ParseExact($timestamps[2], "$datetimeFormat", $null) | Out-Null
    } catch {
        Show-Guard-Message "Timestamps undo clipboard corrupted. Paste new timestamps."
    }
}

function Show-Guard-Message {
    param (
        [string]$Message
    )

    if ($ScriptMode -ieq "Background") {
        Add-Type -AssemblyName PresentationCore,PresentationFramework
        [System.Windows.MessageBox]::Show("$Message", "Timestamp Copy", [System.Windows.MessageBoxButton]::OK, [System.Windows.MessageBoxImage]::Exclamation)
    } elseif (-Not $Quiet) {
        Write-Host "$Message" -ForegroundColor Red
        Pause-Script
    }
    exit 1
}

function Highlight-Diff {
    param (
        [string]$Label,
        [string]$Old,
        [string]$New
    )

    $changed = $false

    Write-Host "$Label $Old (old)"
    Write-Host -NoNewline "$Label "

    $oldParts = $Old -split "[- :]"
    $newParts = $New -split "[- :]"

    function Color-Part {
        param (
            [string]$OldVal,
            [string]$NewVal
        )
        if ("$OldVal" -eq "$NewVal") {
            Write-Host -NoNewline "$NewVal"
        } else {
            Write-Host -NoNewline "$NewVal" -ForegroundColor Green
            Set-Variable -Name changed -Value $true -Scope 1
        }
    }

    Color-Part $oldParts[0] $newParts[0] # y
    Write-Host -NoNewline "-"
    Color-Part $oldParts[1] $newParts[1] # m
    Write-Host -NoNewline "-"
    Color-Part $oldParts[2] $newParts[2] # d
    Write-Host -NoNewline " "
    Color-Part $oldParts[3] $newParts[3] # H
    Write-Host -NoNewline ":"
    Color-Part $oldParts[4] $newParts[4] # M
    Write-Host -NoNewline ":"
    Color-Part $oldParts[5] $newParts[5] # S

    if ($changed) {
        Write-Host " (new)" -ForegroundColor Green
    } else {
        Write-Host " (new)"
    }
}

Main
