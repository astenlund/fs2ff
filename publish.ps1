param (
    [ValidateSet('Debug','Release')]
    [string]$Configuration = 'Release',
    [ValidateSet('win-x64','win10-x64')]
    [string]$Runtime = 'win-x64',
    [Parameter(Mandatory)]
    [string]$Version,
    [ValidateSet('quiet','minimal','normal','detailed','diagnostic')]
    [string]$Verbosity = "minimal",
    [switch]$UseR2R
)

$PublishDir = "publish"

if (Test-Path $PublishDir) { Remove-Item $PublishDir -Recurse -Verbose }

dotnet restore

dotnet publish `
    -o $PublishDir `
    -r $Runtime `
    -c $Configuration `
    -v $Verbosity `
    /p:DebugType=none `
    /p:DebugSymbols=false `
    /p:SelfContained=true `
    /p:PublishSingleFile=true `
    /P:PublishReadyToRun=$UseR2R `
    /p:PublishReadyToRunShowWarnings=$UseR2R `
    /p:Version=$Version

Get-Item "$PublishDir\fs2ff.exe" |
    Select-Object Name,
        @{ Name = 'Version'; Expression = { $_.VersionInfo.ProductVersion }},
        @{ Name = 'Size'; Expression = { "{0:f0} MB`r`n" -f ($_.Length / 1MB) }} |
    Format-List
