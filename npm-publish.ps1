$version = $env:APPVEYOR_BUILD_VERSION;
$vMessage = "version: $version"
$token = $env:npm_token
"//registry.npmjs.org/:_authToken=$token" | Out-File (Join-Path $ENV:APPVEYOR_BUILD_FOLDER ".npmrc") -Encoding UTF8