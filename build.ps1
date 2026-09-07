$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'dtdash.csproj'
$projectText = Get-Content $project -Raw
$versionMatch = [regex]::Match($projectText, '<Version>(\d+)\.(\d+)\.(\d+)</Version>')
if (-not $versionMatch.Success) { throw 'Could not find project version.' }
$version = "$($versionMatch.Groups[1].Value).$($versionMatch.Groups[2].Value).$($versionMatch.Groups[3].Value)"
dotnet build $project -c Release
if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE." }
$exe = (Resolve-Path .\bin\Release\net10.0\dtdash.exe).Path
$desktop = [Environment]::GetFolderPath('Desktop')
$shortcutPath = Join-Path $desktop 'DTDash.lnk'
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $exe
$shortcut.WorkingDirectory = Split-Path $exe
$shortcut.Description = 'Run DTDash'
$shortcut.IconLocation = Join-Path $PSScriptRoot 'redicon.png'
$shortcut.Save()
Write-Host "Built $exe version $version"
Write-Host "Shortcut $shortcutPath"

$nextPatch = [int]$versionMatch.Groups[3].Value + 1
$nextVersion = "$($versionMatch.Groups[1].Value).$($versionMatch.Groups[2].Value).$nextPatch"
$updated = $projectText -replace "<Version>$version</Version>", "<Version>$nextVersion</Version>"
$updated = $updated -replace "<AssemblyVersion>$version\.0</AssemblyVersion>", "<AssemblyVersion>$nextVersion.0</AssemblyVersion>"
$updated = $updated -replace "<FileVersion>$version\.0</FileVersion>", "<FileVersion>$nextVersion.0</FileVersion>"
Set-Content $project -Value $updated -NoNewline
Write-Host "Next build version $nextVersion"
