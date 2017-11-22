var exec = require('child_process').exec;
var glob = require('glob');
var generate = function (options, callback) {
    //console.log(__dirname);
    //console.log(__filename);
    //console.log(process.cwd());
    var cmd = `dotnet "${__dirname}/program/WebTyped.dll" generate ${options.sourceFiles.map(sf => "-sf " + sf).join(" ")} -od ${options.outDir} ${options.trim.map(t => "-t " + t).join(" ")}` + (options.clear ? " -c" : "");
    //console.log("running webtyped");
    var e = exec(cmd, err => {
        //console.log("webtyped completed");
        if (callback) {
            //console.log("calling callback");
            callback();
        }
    });
    e.stdout.on('data', m => console.log(m));
    e.stderr.on('data', m => console.log(m));
    //console.log("running webtyped");
    //exec(cmd, {
    //    stdio: "inherit"
    //});
    //console.log("webtyped completed");
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
            files.map(file => allFiles.push(file.replace(/\//g, '\\')));
        }
        return allFiles;
    }


    //compiler.plugin("compile", function (params) {
    //    console.log("The compiler is starting to compile...");
    //});

    //compiler.plugin("compilation", function (compilation) {
    //    //console.log("The compiler is starting a new compilation...");
    //    //console.log(compilation.fileDependencies);
    //    //compilation.fileDependencies = getFiles();
    //    //console.log("watching " + compilation.fileDependencies);
    //    generate(options);

    //    //compilation.plugin("optimize", function () {
    //    //    console.log("The compilation is starting to optimize files...");
    //    //});
    //});
    var runs = 0;
    function runGenerate(callback) {
        console.log("running webtyped (" + ++runs + ")");
        generate(options, function () {
            console.log("webtyped finished");
            if (callback) {
                callback();
            }
        });
    }
    runGenerate();
    compiler.plugin("invalid", function (fileName, changeTime) {
        if (currentFiles.some(f => f == fileName)) {
            runGenerate();
        }
    });
    //compiler.plugin("before-compile", function (compilationParams, callback) {
    //    console.log("running webtyped (" + ++runs + ")");
    //    generate(options, function () {
    //        console.log("webtyped finished");
    //        callback();
    //    });
    //});
    var currentFiles = [];
    compiler.plugin("after-compile", function (compilation, callback) {
        console.log("after-compile");
        //var files = getFiles();
        //console.log(files);
        currentFiles = getFiles();
        currentFiles.forEach(f => compilation.fileDependencies.unshift(f));
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