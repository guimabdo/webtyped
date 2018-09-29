#!/usr/bin/env node
var pkg = require('./package.json');
console.log(`WebTyped - v.${pkg.version}`);

var exec = require('child_process').exec;
var cmd = `dotnet "${__dirname}/program/WebTyped.Cli.dll" ${process.argv.splice(2)}`;
var e = exec(cmd, err => { });
e.stdout.pipe(process.stdout);
e.stderr.pipe(process.stderr);
