var exec = require('child_process').exec;

exports.generate = function (config) {
    console.log(__dirname);
    console.log(__filename);
    console.log(process.cwd());
    var cmd = `dotnet "${__dirname}/program/WebTyped.dll" generate ${config.sourceFiles.map(sf => "-sf " + sf).join(" ")} -od ${config.outDir}`;
    var e = exec(cmd, err => console.log(err));
    e.stdout.on('data', m => console.log(m));
    e.stderr.on('data', m => console.log(m));
//"--controllers", "../../../../WebTyped.Example.Web/Controllers/**/*.cs",
//				"--models", "../../../../WebTyped.Example.Web/Models/**/*.cs",
//				"--output", "../../../../WebTyped.Example.Web/ClientApp/app/webApi/"

    //console.log("Chamando a partir do node um app netcore que le um arquivo .cs e printa");
    //var e = exec(`dotnet "${__dirname}/bin/RoslynTests.dll" "${filesAndPaths}"`);
    //e.stdout.on('data', m => console.log(m));
    //e.stderr.on('data', m => console.log(m));
    //console.log(text);
};