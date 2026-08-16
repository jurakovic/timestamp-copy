[CmdletBinding(PositionalBinding=$false)]
Param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [string]$OutputPath,
    [switch]$Zip,
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

if (-Not $OutputPath) {
    $OutputPath = "$PSScriptRoot\publish\$Runtime"
}

# Both executables publish into one folder so they share a single copy of the trimmed
# runtime. The folder is the unit of deployment - tscp.exe on its own is only the apphost
# and fails with "The application to execute does not exist: tscp.dll".
#
# They also have to keep identical trim closures, since whichever publishes second overwrites
# the other's framework assemblies. That is why both entry points are one line over Core.Host.
$projects = @(
    "$PSScriptRoot\src\tscp\tscp.csproj"
    "$PSScriptRoot\src\tscpw\tscpw.csproj"
)

if (-Not $SkipTests) {
    Write-Host "Running tests..."
    dotnet test "$PSScriptRoot\tests\TimestampCopy.Tests\TimestampCopy.Tests.csproj" -c "$Configuration"
    if ($LASTEXITCODE -ne 0) { throw "Tests failed" }
    Write-Host ""
}

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

$size = "{0:N1} MB" -f ((Get-ChildItem -Recurse -File "$OutputPath" | Measure-Object -Property Length -Sum).Sum / 1MB)

Write-Host ""
Write-Host "Output: $OutputPath ($size)" -ForegroundColor Green

if ($Zip) {
    $version = (Get-Item "$OutputPath\tscp.exe").VersionInfo.ProductVersion
    $zipPath = "$PSScriptRoot\publish\TimestampCopy-$version-$Runtime.zip"

    if (Test-Path -Path "$zipPath") {
        Remove-Item -Force -Path "$zipPath"
    }

    # The zip contains the folder's contents, not the folder itself, so unzipping into a
    # directory of the user's choosing gives them the layout the installer expects.
    Compress-Archive -Path "$OutputPath\*" -DestinationPath "$zipPath" -CompressionLevel Optimal

    $zipSize = "{0:N1} MB" -f ((Get-Item "$zipPath").Length / 1MB)
    Write-Host "Zip:    $zipPath ($zipSize)" -ForegroundColor Green
}

Write-Host ""
Write-Host "To install, run from an elevated prompt:"
Write-Host "  $OutputPath\tscp.exe -i"
