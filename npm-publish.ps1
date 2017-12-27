"//registry.npmjs.org/:_authToken=$($ENV:npm_token)" | Out-File (Join-Path $ENV:APPVEYOR_BUILD_FOLDER ".npmrc") -Encoding UTF8
set-location ./src/WebTyped.Npm/common
npm version $ENV:version -m 'v$($ENV:version)'
npm publish
set-location ../fetch
npm version $ENV:version -m 'v$($ENV:version)'
npm publish
set-location ../jquery
npm version $ENV:version -m 'v$($ENV:version)'
npm publish
set-location ../angular
npm version $ENV:version -m 'v$($ENV:version)'
npm publish
set-location ../common
npm version $ENV:version -m 'v$($ENV:version)'
npm publish