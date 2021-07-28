const path = require("path");
const fs = require("fs");

const PnpWebpackPlugin = require(`pnp-webpack-plugin`);

const babelConfig = JSON.parse(fs.readFileSync(path.resolve(__dirname, "../.babelrc")));

module.exports = {
    stories: ["../stories/**/*.stories.tsx"],
    addons: [require.resolve("@storybook/addon-actions")],
    webpackFinal: config => {
        config.module.rules = [
            {
                test: /\.[jt]sx?$/,
                use: {
                    loader: require.resolve("babel-loader"),
                    options: babelConfig,
                },
                exclude: /node_modules/,
            },
            {
                test: /\.css$/,
                use: [
                    require.resolve("style-loader"),
                    {
                        loader: require.resolve("css-loader"),
                        options: {
                            modules: {
                                localIdentName: "[name]-[local]-[hash:base64:4]",
                            },
                        },
                    },
                ],
            },
            {
                test: /\.(woff|woff2|eot|ttf|svg|gif|png)$/,
                loader: require.resolve("url-loader"),
            },
        ];

        config.resolve.extensions = [".js", ".jsx", ".ts", ".tsx"];
        config.resolve.plugins = [PnpWebpackPlugin];

        config.resolveLoader.plugins = [PnpWebpackPlugin.moduleLoader(module)];

        return config;
    },
};
