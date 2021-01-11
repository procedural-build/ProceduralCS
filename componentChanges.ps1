git fetch --prune --unshallow
$tag =  git describe --abbrev=0 --tags
$changes = git diff --compact-summary $tag ComputeGH/CFD ComputeGH/Utils

$newComponents = ""
$changedComponents = ""
foreach ($line in $changes){
    if ($line.Contains("new")){
        $line = $line -replace "ComputeGH\/(Utils|CFD)\/GH",""
        $newComponents += ' * ' + ($line -replace ".cs.*", "") + '%0A'
    }
    if ($line.Contains("ComputeGH")) {
        $line = $line -replace "ComputeGH\/(Utils|CFD)\/GH",""
        $changedComponents += ' * ' + ($line -replace ".cs.*", "") + '%0A'
    }
    
}

echo "NEW COMPONENTS"
$newComponents
echo "UPDATED COMPONENTS"
$changedComponents

echo NEW_COMPONENTS=$newComponents | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append
echo UPDATED_COMPONENTS=$changedComponents | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append