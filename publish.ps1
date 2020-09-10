param (
    [ValidateSet('Debug','Release')]
    [string]$Configuration = 'Release',
    [ValidateSet('win-x64','win10-x64')]
    [string]$Runtime = 'win-x64',
    [switch]$NoClean,
    [switch]$RunAfterBuild
)

$PublishDir = ".\bin\$Configuration\netcoreapp3.1\$Runtime\publish"

if (!$NoClean) {
    dotnet clean -r $Runtime -c $Configuration
    if (Test-Path $PublishDir) { rm $PublishDir -Recurse -Verbose }
}

dotnet restore

dotnet publish `
    -r $Runtime `
    -c $Configuration `
    /p:DebugType=None `
    /p:DebugSymbols=False `
    /p:PublishSingleFile=true

ls $PublishDir
gi "$PublishDir\fs2ff.exe" | % { "{0:f0} MB`r`n" -f ($_.Length / 1MB) }

if ($RunAfterBuild) {
    & "$PublishDir\fs2ff.exe"
}
