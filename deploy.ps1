#& "C:\Program Files\Rhino 6\System\yak.exe" login
$yak = "C:\Program Files\Rhino 7 WIP\System\yak.exe"
$name = & $yak search ProceduralCS
$name -match "[0-9.]+"
$currentVersion = $Matches[0]
$year, $month, [int]$buildNumber = $currentVersion.split('.')
$YearMonth = Get-Date -Format "yyyy.M"
$buildNumber = $buildNumber + 1
$newVersion = "$($YearMonth).$($buildNumber)"
cd "$($PSScriptRoot)\dist"
& $yak build