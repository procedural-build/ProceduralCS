# BUILD YAK PACKAGE
cd $env:DIST_PATH\
..\yak.exe build --platform win

# COPY TO RHINO 7
$rh6 = (Get-ChildItem . -Filter *.yak)[0]
$rh7 = $rh6 -replace "rh6_20", "rh7_0"
Copy-Item -Path $rh6 -Destination $rh7

# ECHO FILES
Get-ChildItem . -Filter *.yak