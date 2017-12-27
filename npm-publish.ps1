$version = $env:APPVEYOR_BUILD_VERSION;
$vMessage = "version: $version"
$token = $env:npm_token
"//registry.npmjs.org/:_authToken=$token" | Out-File (Join-Path $ENV:APPVEYOR_BUILD_FOLDER ".npmrc") -Encoding UTF8
Write-Host $vMessage
Write-Host "showing npm v"
npm -v
set-location ./src/WebTyped.Npm/common
npm version $version -m $vMessage
npm pack
npm publish
set-location ../fetch
npm version $version -m $vMessage
npm pack
npm publish
set-location ../jquery
npm version $version -m $vMessage
npm pack
npm publish
set-location ../angular
npm version $version -m $vMessage
npm pack
npm publish
set-location ../common
npm version $version -m $vMessage
npm pack
npm publish