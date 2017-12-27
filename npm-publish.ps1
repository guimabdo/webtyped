"//registry.npmjs.org/:_authToken=`$`{npm_token`}" | Out-File (Join-Path $ENV:APPVEYOR_BUILD_FOLDER ".npmrc") -Encoding UTF8
$vMessage = "version: $env:version"
Write-Host $vMessage
set-location ./src/WebTyped.Npm/common
npm version $env:version -m 'v$($ENV:version)'
npm publish
set-location ../fetch
npm version $env:version -m 'v$($ENV:version)'
npm publish
set-location ../jquery
npm version $env:version -m 'v$($ENV:version)'
npm publish
set-location ../angular
npm version $env:version -m 'v$($ENV:version)'
npm publish
set-location ../common
npm version $env:version -m 'v$($ENV:version)'
npm publish