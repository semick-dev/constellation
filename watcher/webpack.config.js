// webpack.config.js
const path = require("path");
const webpack = require('webpack');

module.exports = {
  entry: "./src/watcher.ts",
  module: {
    rules: [
      {
        test: /\.ts$/,
        use: "ts-loader",
        exclude: /node_modules/
      }
    ]
  },
  plugins: [
    new webpack.ProvidePlugin({
        Buffer: ['buffer', 'Buffer'],
    }),
    new webpack.ProvidePlugin({
        process: 'process/browser',
    }),
  ],
  resolve: {
    extensions: [".ts", ".js"],
  },
  output: {
    filename: "watcher.js",
    library: "watcher",
    libraryTarget: "umd",
    path: path.resolve(__dirname, "dist")
  }
};