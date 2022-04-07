import 'source-map-support/register'
import * as cdk from 'aws-cdk-lib'
import { MainStack } from './src/stacks'

const app = new cdk.App()

new MainStack(app, 'Main', {
  env: {
    region: process.env.CDK_DEFAULT_REGION,
    account: process.env.CDK_DEFAULT_ACCOUNT
  }
})

app.synth()
