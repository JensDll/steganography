$solutionFile = Join-Path $PSScriptRoot .. api.sln

jb cleanupcode --profile=Format $solutionFile
