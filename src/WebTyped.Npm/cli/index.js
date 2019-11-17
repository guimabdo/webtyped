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

        for (let f of files) {
            let filePath = args[2] + path.sep + f.path;
            filePath = filePath.replace(/\\/g, path.sep);
            filePath = filePath.replace(/\//g, path.sep);
            console.log(filePath);
            //fs.writeFile(args[2] + '/' + f.path, f.content, {}, () => { });
            //fs.writeFileSync(path, f.content);
            writeFileSyncRecursive(filePath, f.content, 'utf-8');
        }

        
    }
});

e.stdout.pipe(process.stdout);
e.stderr.pipe(process.stderr);


function writeFileSyncRecursive(filename, content, charset) {
    
    const folders = filename.split(path.sep).slice(0, -1);
    if (folders.length) {
        // create folder path if it doesn't exist
        folders.reduce((last, folder) => {
            const folderPath = last ? last + path.sep + folder : folder;
            if (!fs.existsSync(folderPath)) {
                fs.mkdirSync(folderPath);
            }
            return folderPath;
        });
    }
    fs.writeFileSync(filename, content, charset);
}