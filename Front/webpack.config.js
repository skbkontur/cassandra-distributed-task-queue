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
    "webworms-admintools-bundle": "./../../WebWorms/Web/webworms-admintools-bundle.js",
  },
  output: {
    path: './dist',
    publicPath: '/dist/',
    filename: output_filename('js')
  },
  module: {
    loaders: [
      {
        test: /\.(css|less)$/,
        loader: ExtractTextPlugin.extract('style-loader', 'css-loader')
      },
      {
        test: /\.(woff|eot|png|gif|ttf)$/, 
        loader: "file-loader"
      }
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