var exec = require('child_process').exec;
var glob = require('glob');
var generate = function (options, callback) {
    var cmd =
        `dotnet "${__dirname}/program/WebTyped.dll" generate ` +
        `${options.sourceFiles.map(sf => "-s " + sf).join(" ")} ` +
        `-o ${options.outDir} ` +
        `${(options.trim ? options.trim.map(t => "-t " + t).join(" ") : "")}` + 
(options.clear ? " -c" : "") + " " +
(options.serviceMode ? `-S ${options.serviceMode}` : "") + " " +
(options.baseModule ? `-b ${options.baseModule}` : "") + " " +
        (options.keepPropsCase ? " --keepPropsCase" : "");
    var e = exec(cmd, err => {
        if (callback) {
            callback();
        }
    });
    e.stdout.on('data', m => console.log(m));
    e.stderr.on('data', m => console.log(m));
};

function WebTypedPlugin(options) {
    console.log("constructing webtyped plugin");
    this.options = options;
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
            console.log(fileName + " changed");
            runGenerate();
        }
    });
    var currentFiles = [];
    compiler.plugin("after-compile", function (compilation, callback) {
        currentFiles = getFiles();
        currentFiles.forEach(f => compilation.fileDependencies.unshift(f));
        callback();
    });
};

module.exports = {
    WebTypedPlugin: WebTypedPlugin
};