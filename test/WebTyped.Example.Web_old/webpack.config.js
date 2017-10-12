/// <binding ProjectOpened='Watch - Development' />
var webpack = require('webpack');
var path = require('path');
var plugins = [
    new webpack.optimize.UglifyJsPlugin({
        sourceMap: false,
        compress: { warnings: false }
    })
];

module.exports = {
    devtool:'eval',
    entry: [
        './Scripts/main.ts',
    ],
    output: {
        path: path.resolve('./wwwroot'),
        filename: 'bundle.js'

    },
    module: {
        loaders: [
            {
                test: /\.ts$/,
                use: ['awesome-typescript-loader']
            }
        ]
    },
    resolve: {
        extensions: ['*', '.js', '.ts'],
        modules: [path.resolve('node_modules'), path.resolve('Scripts')]
    },
    plugins: plugins
};