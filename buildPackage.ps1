# BUILD YAK PACKAGE
cd $env:DIST_PATH\
..\yak.exe build --platform win

# COPY TO RHINO 7
$rh6 = (Get-ChildItem . -Filter *.yak)[0]
$rh7 = $rh6 -replace "rh6_20", "rh7_0"
Rename-Item -Path $rh6 -NewName $rh7
