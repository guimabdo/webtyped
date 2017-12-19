declare class WebTypedPlugin extends Plugin {
    constructor(options: WebTypedPlugin.Options);
}

declare namespace WebTypedPlugin {
    interface Options {
        /**
         * Csharp source files (models and controllers)
         */
        sourceFiles: [];

        /**
         * Output directory
         */
        outDir: string;

        /**
        * Starting names for trimming modules
        */
        trim: [];
        
        /**
         * Clear all .ts files from output dir that are note generated
         */
        clear: boolean;

        /**
         * Keep case properties for models
         */
        keepPropsCase: boolean;
    }
}
