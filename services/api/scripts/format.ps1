$solution = Join-Path $PSScriptRoot .. api.sln

jb cleanupcode --profile=Format $solution
