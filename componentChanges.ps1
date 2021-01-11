#git fetch --prune --unshallow
$tag =  git describe --abbrev=0 --tags
$changes = git diff --compact-summary $tag ComputeGH/CFD ComputeGH/Utils

$newComponents = ""
$changedComponents = ""
foreach ($line in $changes){
    if ($line.Contains("new")){
        $line = $line -replace "ComputeGH\/(Utils|CFD)\/GH",""
        $newComponents += " * " + ($line -replace ".cs.*", "") + "`n"
    }
    if ($line.Contains("ComputeGH")) {
        $line = $line -replace "ComputeGH\/(Utils|CFD)\/GH",""
        $changedComponents += " * " + ($line -replace ".cs.*", "") + "`n"
    }
    
}

echo "NEW COMPONENTS"
$newComponents
echo "UPDATED COMPONENTS"
$changedComponents

$version = "2021.1.1"
$releaseFile = ".\.github\releases\release1.md"
$release = Get-Content $releaseFile
$release = $release.Replace("{{ NEW_COMPONENTS }}", $newComponents)
$release = $release.Replace("{{ UPDATED_COMPONENTS }}", $changedComponents)
$release = $release.Replace("{{ RELEASE_VERSION }}", $version)

echo $release | Out-File -FilePath $releaseFile -Encoding utf8

#echo NEW_COMPONENTS=$newComponents | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append
#echo UPDATED_COMPONENTS=$changedComponents | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append