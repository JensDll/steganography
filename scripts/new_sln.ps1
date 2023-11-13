$root = Join-Path $PSScriptRoot ..
$name = Get-Item $root | Select-Object -ExpandProperty Name
$solutionFile = Join-Path $root "$name.sln"

Remove-Item $solutionFile -ErrorAction SilentlyContinue

dotnet new sln --output "$root"
dotnet sln "$solutionFile" add $(Get-ChildItem -Recurse '**/*.csproj')
