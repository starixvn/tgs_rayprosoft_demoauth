import { Configuration } from 'webpack';
import { CustomWebpackBrowserSchema, TargetOptions } from '@angular-builders/custom-webpack';
import * as HtmlWebpackPlugin from 'html-webpack-plugin';

export default (config: Configuration, options: CustomWebpackBrowserSchema, targetOptions: TargetOptions) => {
  config.plugins?.push(
    new HtmlWebpackPlugin({
      template: 'src/login.html',
      filename: 'login.html',
      inject: false
    }),
  );

  return config;
};
