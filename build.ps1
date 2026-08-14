[CmdletBinding(PositionalBinding=$false)]
Param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

if (-Not $OutputPath) {
    $OutputPath = "$PSScriptRoot\publish\$Runtime"
}

# Both executables publish into one folder so they share a single copy of the trimmed
# runtime. The folder is the unit of deployment - tscp.exe on its own is only the apphost
# and fails with "The application to execute does not exist: tscp.dll".
$projects = @(
    "$PSScriptRoot\src\tscp\tscp.csproj"
    "$PSScriptRoot\src\tscpw\tscpw.csproj"
)

if (Test-Path -Path "$OutputPath") {
    Remove-Item -Recurse -Force -Path "$OutputPath"
}

foreach ($project in $projects) {
    Write-Host "Publishing $(Split-Path -Leaf $project)..."
    dotnet publish "$project" `
        -c "$Configuration" `
        -r "$Runtime" `
        --self-contained true `
        -p:PublishReadyToRun=true `
        -p:PublishTrimmed=true `
        -p:PublishDir="$OutputPath\"
    if ($LASTEXITCODE -ne 0) { throw "Publish failed for $project" }
}

Remove-Item -Force -Path "$OutputPath\*.pdb"

# Staged alongside the binaries until the port is complete: the script still handles the
# actions that are not native yet, and Add-MenuItem picks up "$PSScriptRoot\tscp.exe" only
# when the script sits in the same folder. See PLAN.md.
Copy-Item -Force -Path "$PSScriptRoot\TimestampCopy.ps1" -Destination "$OutputPath"
Copy-Item -Force -Path "$PSScriptRoot\tscp.ico" -Destination "$OutputPath"

$size = "{0:N1} MB" -f ((Get-ChildItem -Recurse -File "$OutputPath" | Measure-Object -Property Length -Sum).Sum / 1MB)

Write-Host ""
Write-Host "Output: $OutputPath ($size)" -ForegroundColor Green
Write-Host ""
Write-Host "To install, run from an elevated prompt:"
Write-Host "  $OutputPath\TimestampCopy.ps1 -i"
