var gen = require('@guimabdo/webtyped-generator');
gen.generate({
    sourceFiles: ["Controllers/**/*.cs", "Models/**/*.cs"],
    outDir:"ClientApp/app/webApi/",
    someCallback: str => { console.log(str); },
    trim: [""],
    clear: true
});