declare class WebTypedPlugin extends Plugin {
    constructor(options: WebTypedPlugin.Options);
}

declare namespace WebTypedPlugin {
    interface Options {
        /**
         * Csharp source files (models and controllers)
         */
        sourceFiles: Array<string>;

        /**
         * Output directory
         */
        outDir: string;

        /**
        * Starting names for trimming modules
        */
        trim: Array<string>;
        
        /**
         * Clear all .ts files from output dir that are note generated
         */
        clear: boolean;

        serviceMode: "angular" | "jquery" | "fetch";

        baseModule: string;

        /**
         * Keep case properties for models
         */
        keepPropsCase: boolean;
    }
}

export = {
    WebTypedPlugin: WebTypedPlugin
};