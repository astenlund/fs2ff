param (
    [ValidateSet('Debug','Release')]
    [string]$Configuration = 'Release',
    [ValidateSet('win-x64','win10-x64')]
    [string]$Runtime = 'win-x64',
    [Parameter(Mandatory)]
    [string]$Version,
    [switch]$NoClean,
    [switch]$RunAfterBuild
)

$PublishDir = ".\bin\$Configuration\netcoreapp3.1\$Runtime\publish"

if (!$NoClean) {
    dotnet clean -r $Runtime -c $Configuration
    if (Test-Path $PublishDir) { Remove-Item $PublishDir -Recurse -Verbose }
}

dotnet restore

dotnet publish `
    -r $Runtime `
    -c $Configuration `
    /p:DebugType=none `
    /p:DebugSymbols=false `
    /p:SelfContained=true `
    /p:PublishSingleFile=true `
    /P:PublishReadyToRun=true `
    /p:PublishReadyToRunShowWarnings=true `
    /p:Version=$Version

Get-Item "$PublishDir\fs2ff.exe" |
    Select-Object Name,
        @{ Name = 'Version'; Expression = { $_.VersionInfo.ProductVersion }},
        @{ Name = 'Size'; Expression = { "{0:f0} MB`r`n" -f ($_.Length / 1MB) }} |
    Format-List

if ($RunAfterBuild) {
    & "$PublishDir\fs2ff.exe"
}
