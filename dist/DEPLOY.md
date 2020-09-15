# DEPLOY WITH YAK

## Login to Yak
```
"C:\Program Files\Rhino 6\System\yak.exe" login
```

## Bump version
Bump the version of the ComputeGH plugin in the `dist/manifest.yml`
We version as YEAR.MONTH.BUILD_NUMBER e.g.: 2020.9.1

## Build and Push to Yak
```
cd dist\
"C:\Program Files\Rhino 6\System\yak.exe" build
"C:\Program Files\Rhino 6\System\yak.exe" push proceduralcs-$VERSION.yak
```