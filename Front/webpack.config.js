require("babel-polyfill");

var path = require('path');
var ExtractTextPlugin = require('extract-text-webpack-plugin');

var isProduction = false;
for (var i = 2; i < process.argv.length; i++){
    if (process.argv[i] == '-p'){
        isProduction = true;
        break;
    }
}

var isDevServer = process.argv[1].indexOf('webpack-dev-server') >= 0;

function output_filename(ext) {
  return isDevServer ? ('[name].' + ext) : ('[name].[hash].' + ext);
}
module.exports = {
    context: __dirname,
    entry: {
        "webworms-initializer": "./../../WebWorms/Web/webworms-initializer.js",
        "webworms-bundle": "./../../WebWorms/Web/webworms-bundle.js",
        "remote-task-queue-bundle": "./Statics/remote-task-queue-bundle.js",
    },
    output: {
        path: './dist',
        publicPath: '/dist/',
        filename: output_filename('js')
    },
    module: {
        loaders: [
            {
                test: /\.(css)$/,
                loader: ExtractTextPlugin.extract('style-loader', 'css-loader')
            },
            {
                test: /\.(less)$/,
                loader: ExtractTextPlugin.extract('style-loader', 'css-loader!less-loader')
            },
            {
                test: /\.(woff|woff2|eot|png|gif|ttf)$/,
                loader: "file-loader"
            },
            {
                test: /\.jsx?$/,
                loader: 'babel-loader',
                query: {
                    presets: [
                        require.resolve('./es2015-loose-mode'),
                        require.resolve('babel-preset-stage-0')
                    ]
                }
            },
        ]
    },
    resolveLoader: {
        fallback: path.join(__dirname, 'node_modules')
    },
    plugins: [ 
        new ExtractTextPlugin(output_filename('css')),
        function() {
            if (!isDevServer){
                this.plugin("done", function(stats) {
                    require("fs").writeFileSync(
                    path.join(__dirname, "webpack-assets.json"),
                    JSON.stringify(stats.toJson().assetsByChunkName));
                });          
            }
        }
    ]    
}