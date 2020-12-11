# BUMP VERSION
$yak = "C:\Program Files\Rhino 7 WIP\System\yak.exe"
$name = & $yak search ProceduralCS
$name -match "[0-9.]+"
$currentVersion = $Matches[0]
$year, [int]$month, [int]$buildNumber = $currentVersion.split('.')
$YearMonth = Get-Date -Format "yyyy.M"
$buildNumber = [int](Get-Date -Format "MM") > $month? 0 : $buildNumber + 1
$newVersion = "$($YearMonth).$($buildNumber)"
$versionFile = "$($PSScriptRoot)\ComputeGH\Resources\Version.txt"
echo $newVersion | Out-File -FilePath $versionFile

# BUILD DIST

# BUILD YAK PACKAGE
#cd "$($PSScriptRoot)\dist"
#& $yak build

# PUSH YAK PACKAGE