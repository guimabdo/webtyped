var exec = require('child_process').execSync;
var glob = require('glob');
var generate = function (options) {
    //console.log(__dirname);
    //console.log(__filename);
    //console.log(process.cwd());
    var cmd = `dotnet "${__dirname}/program/WebTyped.dll" generate ${options.sourceFiles.map(sf => "-sf " + sf).join(" ")} -od ${options.outDir} ${options.trim.map(t => "-t " + t).join(" ")}` + (options.clear ? " -c" : "");
    //var e = exec(cmd, err => console.log(err));
    //e.stdout.on('data', m => console.log(m));
    //e.stderr.on('data', m => console.log(m));
    console.log("running webtyped");
    exec(cmd, {
        stdio: "inherit"
    });
    console.log("webtyped completed");
};

function WebTypedPlugin(options) {
    this.options = options;
    // Configure your plugin with options...
}

WebTypedPlugin.prototype.apply = function (compiler) {
    var options = this.options;
    function getFiles() {
        var allFiles = [];
        for (var i = 0; i < options.sourceFiles.length; i++) {
            var sf = options.sourceFiles[i];
            var files = glob.sync(sf, { absolute: true });
            files.map(file => allFiles.push(file));
        }
        return allFiles;
    }


    compiler.plugin("compile", function (params) {
        console.log("The compiler is starting to compile...");
    });

    compiler.plugin("compilation", function (compilation) {
        //console.log("The compiler is starting a new compilation...");
        //console.log(compilation.fileDependencies);
        //compilation.fileDependencies = getFiles();
        //console.log("watching " + compilation.fileDependencies);
        generate(options);

        //compilation.plugin("optimize", function () {
        //    console.log("The compilation is starting to optimize files...");
        //});
    });

    compiler.plugin("after-compile", function (compilation, callback) {
        console.log("after-compile");
        //var files = getFiles();
        //console.log(files);
        getFiles().forEach(f => compilation.fileDependencies.unshift(f.replace(/\//g, '\\')));
        //console.log(compilation.fileDependencies);
        //for (var i = 0; i < options.sourceFiles.length; i++) {
        //    var sf = options.sourceFiles[i];
        //    var files = glob.sync(sf, { absolute: true });
        //    //console.log(files);
        //    files.map(file => {
        //        console.log("adding " + file);
        //        compilation.fileDependencies.push(file);
        //    });
        //    //console.log(files);
        //    //compilation.fileDependencies.push.apply(files);
        //    //console.log(glob.sync(sf));
        //}
        callback();
    });

    //compiler.plugin("emit", function (compilation, callback) {
    //    console.log("The compilation is going to emit files...");
    //    callback();
    //});

    //var watchers = [];
    //compiler.plugin("watch-run", function (compilation, callback) {
    //    console.log("The compilation is going to watch files...");
    //    getFiles().map(f => watchers.push(compiler.watch(f, {})));
    //    callback();
    //});

    //compiler.plugin("watch-close", function (compilation, callback) {
    //    console.log("The compilation is going to stop watch files...");
    //    watchers.forEach(w => w.sto
    //    callback();
    //});
};

module.exports = {
    WebTypedPlugin: WebTypedPlugin
};