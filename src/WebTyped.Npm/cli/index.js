#!/usr/bin/env node
var pkg = require('./package.json');
const path = require('path');
var fs = require('fs');

console.log(`WebTyped - v.${pkg.version}`);

var exec = require('child_process').exec;
let args = process.argv.splice(2);
var cmd = `dotnet "${__dirname}/program/WebTyped.Cli.dll" ${args.join(' ')}`;
var e = exec(cmd, (err, stdout) => {
    if (args[0] === 'generate-with') {
        if (!args[1]) {
            console.log('Generator undefined');
            return;
        }

        if (!args[2]) {
            console.log('Output path undefined');
            return;
        }

        console.log('\n\n\n\x1b[36m%s\x1b[0m: %s', 'Starting custom generator', args[1]);

        //Get generator
        let customGenerator = require(args[1]);

        //Get abstractions path
        let abstractionsPath = stdout.match(/^Abstractions=>(.+)$/m)[1];

        //Read content
        let abstractions = JSON.parse(fs.readFileSync(abstractionsPath));

        //Pass to generator
        let files = customGenerator(abstractions);

        //Clean up dir
        let outDir = args[2];
        let webtypedMemoryFile = outDir + path.sep + '.webtyped';
        if (fs.existsSync(webtypedMemoryFile)) {
            let generatedFiles = fs.readFileSync(webtypedMemoryFile, 'utf8').split(';');
            for (let f of generatedFiles) {
                fs.unlinkSync(f);
            }

        }

        //Read .webtyped memory
        let paths = [];
        
        for (let f of files) {
            let filePath = outDir + path.sep + f.path;
            filePath = filePath.replace(/\\/g, path.sep);
            filePath = filePath.replace(/\//g, path.sep);

            //Create dir
            let parts = filePath.split(path.sep);
            let dir = parts.slice(0, parts.length - 1).join(path.sep);
            fs.mkdirSync(dir, { recursive: true });

            //Save file
            fs.writeFileSync(filePath, f.content);
            paths.push(filePath);
        }

        //Write .webtyped memory
        fs.mkdirSync(outDir, { recursive: true });
        fs.writeFileSync(webtypedMemoryFile, paths.join(';'));
    }
});

e.stdout.pipe(process.stdout);
e.stderr.pipe(process.stderr);