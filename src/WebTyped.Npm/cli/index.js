#!/usr/bin/env node
const { exec } = require('child_process');
const program = require('commander');
const fs = require('fs');
const path = require('path');

const pkg = require('./package.json');
program.version(pkg.version);

program
    .option('-c, --configuration <configuration>', 'Configuration File', 'webtyped.json')
    .action(function (cmd) {
        console.log(pkg.version + '\n\n');

        //Check if webtyped.json exists
        if (!fs.existsSync(cmd.configuration)) {
            console.log('\x1b[31m%s\x1b[0m', '\n    Configuration file not found\n');
            program.outputHelp();
            return;
        }
        //Read webtyped.json
        const config = JSON.parse(fs.readFileSync(cmd.configuration, 'utf8'));

        const args = process.argv.splice(2);
        const dotnet = `dotnet "${__dirname}/program/WebTyped.Cli.dll" ${args.join(' ')}`;
        const e = exec(dotnet, (err, stdout) => {
            if (config.generator) {
                console.log('\n\x1b[36m%s\x1b[0m: %s\n', 'Starting custom generator', config.generator);

                //Get generator
                let customGenerator = require(path.resolve(config.generator));
                const outDir = config.outDir || 'webtyped';

                //Get abstractions path
                let abstractionsPath = '.webtyped-abstractions';

                if (!fs.existsSync(abstractionsPath)) {
                    console.log('\x1b[36m%s\x1b[0m\n', 'Abstractions not found!');
                    return;
                }

                //Read content
                let abstractions = JSON.parse(fs.readFileSync(abstractionsPath));
                fs.unlinkSync(abstractionsPath);

                //Pass to generator
                let files = customGenerator(abstractions);
                console.log('\x1b[36m%s\x1b[0m\n', 'Generator processed');

                //Clean up dir
                let webtypedMemoryFile = outDir + path.sep + '.webtyped';
                if (fs.existsSync(webtypedMemoryFile)) {
                    let generatedFiles = fs.readFileSync(webtypedMemoryFile, 'utf8').split(';');
                    for (let f of generatedFiles) {
                        if (fs.existsSync(f)) {
                            fs.unlinkSync(f);
                        }
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

    });

const parsed = program.parse(process.argv);